using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.Microcks.FileArtifacts;
using Aspire.Hosting.Microcks.MainRemoteArtifacts;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting.Microcks;

/// <summary>
/// Lifecycle hook that initializes Microcks resources after containers are
/// created. When the distributed application is running (not in publish
/// mode), this hook watches the Microcks container logs for readiness,
/// uploads configured artifacts, imports remote artifacts and snapshots,
/// and waits for the service health endpoint to become available.
/// </summary>
/// <param name="loggerFactory">Factory used to create loggers for resources.</param>
/// <param name="resourceLoggerService">Service used to stream resource logs for readiness detection.</param>
/// <param name="executionContext">Execution context describing run/publish mode.</param>
internal sealed class MicrocksResourceLifecycleHook(
    ILoggerFactory loggerFactory,
    ResourceLoggerService resourceLoggerService,
    DistributedApplicationExecutionContext executionContext)
    : IDistributedApplicationLifecycleHook, IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdownCancellationTokenSource = new();

    
    /// <summary>
    /// Called after container resources have been created. For each Microcks
    /// resource this hook will attach a logger, wait for the service to be
    /// ready, and then upload/import configured artifacts and snapshots.
    /// </summary>
    /// <param name="appModel">The distributed application model containing created resources.</param>
    /// <param name="cancellationToken">A token to observe while waiting for resources or performing imports.</param>
    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        // MicrocksResourceLifecycleHook only applies to RunMode
        if (executionContext.IsPublishMode)
        {
            return;
        }

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _shutdownCancellationTokenSource.Token,
            cancellationToken);

        var microcksResources = appModel.GetContainerResources()
            .OfType<MicrocksResource>();

        foreach (var microcksResource in microcksResources)
        {
            microcksResource.SetLogger(loggerFactory);
            var endpoint = microcksResource.GetEndpoint();
            if (endpoint.IsAllocated)
            {
                await GetMicrocksHealthyAsync(microcksResource, cancellationTokenSource.Token);

                // Upload Microcks artifacts
                await MicrocksArtifactUploader.UploadArtifactsAsync(microcksResource, cancellationTokenSource.Token)
                    .ConfigureAwait(false);

                // Import Microcks remote artifacts
                await MicrocksRemoteArtifactImporter.ImportRemoteArtifactsAsync(microcksResource, cancellationTokenSource.Token)
                    .ConfigureAwait(false);

                await MicrocksArtifactImporter.ImportSnapshotsAsync(microcksResource, cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }

        }
    }

    /// <summary>
    /// Waits until the Microcks container emits a startup log line and the
    /// resource health endpoint reports healthy.
    /// </summary>
    /// <param name="microcksResource">The Microcks resource to monitor.</param>
    /// <param name="cancellationToken">A token to cancel waiting early.</param>
    private async Task GetMicrocksHealthyAsync(MicrocksResource microcksResource, CancellationToken cancellationToken)
    {
        try
        {
            // Watch the logs of the Microcks resource until we find the line "Microcks server started"
            await foreach (var batch in resourceLoggerService.WatchAsync(microcksResource).WithCancellation(cancellationToken))
            {
                // Watch for the "Started MicrocksApplication" log line
                if (batch.Any(line => line.Content.Contains("Started MicrocksApplication", StringComparison.OrdinalIgnoreCase)))
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation while listening to logs
        }

        await microcksResource.WaitForHealthAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels any pending watch operations and disposes the internal
    /// cancellation token source used by this hook.
    /// </summary>
    /// <returns>A value task that completes when disposal is finished.</returns>
    public async ValueTask DisposeAsync()
    {
        await _shutdownCancellationTokenSource.CancelAsync();
    }
}