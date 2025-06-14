using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using System.Linq;
using Aspire.Hosting.ApplicationModel;
using System.Net;

namespace Aspire.Hosting.Microcks.MainArtifacts;

/// <summary>
/// Handles uploading artifacts to a Microcks instance.
/// </summary>
internal static class MicrocksArtifactUploader
{

    public static async Task UploadAsync(MicrocksResource resource, CancellationToken token)
    {
        var mainArtifactsAnnotations = resource.Annotations.OfType<MainArtifactAnnotation>();
        if (mainArtifactsAnnotations == null || !mainArtifactsAnnotations.Any())
            return;

        var endpoint = resource.GetEndpoint("http");
        var hostUrl = endpoint.Url;
        var client = RestService.For<IUploadArtifactEndpoint>(hostUrl);

        foreach (var artifactPath in mainArtifactsAnnotations.Select(a => a.SourcePath))
        {
            if (!File.Exists(artifactPath))
                throw new FileNotFoundException($"Artifact file not found: {artifactPath}");

            await using var stream = File.OpenRead(artifactPath);
            var fileName = Path.GetFileName(artifactPath);
            var content = new StreamPart(stream, fileName);
            var result = await client.UploadArtifactAsync(true, content, token);
            if (result.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException($"Failed to upload artifact '{fileName}'");
        }
    }
}
