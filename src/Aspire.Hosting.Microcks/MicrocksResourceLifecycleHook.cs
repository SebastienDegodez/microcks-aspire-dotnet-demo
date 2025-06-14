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

internal sealed class MicrocksResourceLifecycleHook(
    ILoggerFactory loggerFactory,
    DistributedApplicationExecutionContext executionContext)
    : IDistributedApplicationLifecycleHook, IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdownCancellationTokenSource = new();

    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        // MicrocksResourceLifecycleHook only applies to RunMode
        if (executionContext.IsPublishMode)
        {
            return;
        }
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCancellationTokenSource.Token, cancellationToken);

        var microcksResources = appModel.GetContainerResources()
            .OfType<MicrocksResource>();

        foreach (var microcksResource in microcksResources)
        {
            microcksResource.SetLogger(loggerFactory);
            var endpoint = microcksResource.GetEndpoint();
            if (endpoint.IsAllocated)
            {
                await microcksResource.WaitForHealthAsync(cancellationTokenSource.Token)
                    .ConfigureAwait(false);

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
    /// Disposes the resources used by the lifecycle hook.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _shutdownCancellationTokenSource.CancelAsync();
    }
}