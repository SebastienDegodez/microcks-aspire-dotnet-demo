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
/// Handles uploading artifacts to a Microcks instance.
/// </summary>
internal static class MicrocksArtifactUploader
{

    public static async Task UploadArtifactsAsync(MicrocksResource microcksResource, CancellationToken cancellationToken)
    {
        var mainArtifactsAnnotations = microcksResource.Annotations.OfType<MainArtifactAnnotation>();
        if (mainArtifactsAnnotations == null || !mainArtifactsAnnotations.Any())
        {
            return;
        }

        foreach (var artifactPath in mainArtifactsAnnotations.Select(a => a.SourcePath))
        {
            await UploadWithRetryAsync(microcksResource, artifactPath, true, cancellationToken);
        }

        // Upload secondary artifacts
        var secondaryArtifactsAnnotations = microcksResource.Annotations.OfType<SecondaryArtifactAnnotation>();
        foreach (var artifactPath in secondaryArtifactsAnnotations.Select(a => a.SourcePath))
        {
            await UploadWithRetryAsync(microcksResource, artifactPath, false, cancellationToken);
        }
    }

    private static async Task UploadWithRetryAsync(MicrocksResource microcksResource, string artifactPath, bool isMainArtifact, CancellationToken cancellationToken)
    {
        const int retryCount = 5;
        var retryDelay = TimeSpan.FromMilliseconds(100);

        for (var attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                await UploadArtifactAsync(microcksResource, artifactPath, isMainArtifact, cancellationToken);
                return;
            }
            catch (HttpRequestException ex) when (attempt < retryCount)
            {
                microcksResource.Logger?.LogWarning(ex, "Transient error uploading artifact '{FileName}', attempt {Attempt}", Path.GetFileName(artifactPath), attempt);
                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }

    private static async Task UploadArtifactAsync(
        MicrocksResource microcksResource,
        string artifactPath,
        bool isMainArtifact,
        CancellationToken cancellationToken)
    {
        var client = microcksResource.Client.Value;
        using (var stream = File.OpenRead(artifactPath))
        {
            var fileName = Path.GetFileName(artifactPath);
            var content = new StreamPart(stream, fileName, "application/json");

            var result = await client.UploadArtifactAsync(isMainArtifact, content, cancellationToken)
                .ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                microcksResource.Logger?.LogError("Failed to upload artifact '{FileName}' with status code {StatusCode}", fileName, result.StatusCode);
                throw new InvalidOperationException($"Failed to upload artifact '{fileName}'");
            }

            microcksResource.Logger?.LogInformation("Artifact '{FileName}' uploaded successfully", fileName);
        }
    }
}
