using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting.Microcks.FileArtifacts;

/// <summary>
/// Handles importing artifacts to a Microcks instance.
/// </summary>
internal static class MicrocksArtifactImporter
{
    public static async Task ImportSnapshotsAsync(MicrocksResource microcksResource, CancellationToken cancellationToken)
    {
        var snapshotAnnotations = microcksResource.Annotations
            .OfType<SnapshotsAnnotation>();
        
        foreach (var artifactPath in snapshotAnnotations.Select(a => a.SnapshotsFilePath))
        {
            await ImportWithRetryAsync(microcksResource, artifactPath, cancellationToken);
        }
    }

    private static async Task ImportWithRetryAsync(MicrocksResource microcksResource, string artifactPath, CancellationToken cancellationToken)
    {
        const int retryCount = 5;
        var retryDelay = TimeSpan.FromMilliseconds(100);

        for (var attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                await ImportArtifactAsync(microcksResource, artifactPath, cancellationToken);
                return;
            }
            catch (HttpRequestException ex) when (attempt < retryCount)
            {
                microcksResource.Logger?.LogWarning(ex, "Transient error importing artifact '{FileName}', attempt {Attempt}", Path.GetFileName(artifactPath), attempt);
                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }

    private static async Task ImportArtifactAsync(
        MicrocksResource microcksResource,
        string artifactPath,
        CancellationToken cancellationToken)
    {
        var client = microcksResource.Client.Value;
        using (var stream = File.OpenRead(artifactPath))
        {
            var fileName = Path.GetFileName(artifactPath);
            var content = new StreamPart(stream, fileName, "application/json");

            var result = await client.ImportArtifactAsync(content, cancellationToken)
                .ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                microcksResource.Logger?.LogError("Failed to import artifact '{FileName}' with status code {StatusCode}", fileName, result.StatusCode);
                throw new InvalidOperationException($"Failed to import artifact '{fileName}'");
            }

            microcksResource.Logger?.LogInformation("Artifact '{FileName}' imported successfully", fileName);
        }
    }
}
