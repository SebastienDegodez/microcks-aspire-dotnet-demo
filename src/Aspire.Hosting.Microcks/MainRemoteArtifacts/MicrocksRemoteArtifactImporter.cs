using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Aspire.Hosting.Microcks.MainRemoteArtifacts;

/// <summary>
/// Handles importing remote artifacts into Microcks.
/// </summary>
internal static class MicrocksRemoteArtifactImporter
{
    public static async Task ImportRemoteArtifactsAsync(MicrocksResource microcksResource, CancellationToken token)
    {
        var remoteArtifactUrls = microcksResource.Annotations
            .OfType<MainRemoteArtifactAnnotation>();
        
        if (remoteArtifactUrls == null || !remoteArtifactUrls.Any())
        {
            return;
        }

        var client = microcksResource.Client.Value;

        foreach (var remoteUrl in remoteArtifactUrls.Select(a => a.RemoteArtifactUrl))
        {
            var result = await client.DownloadArtifactAsync(true, remoteUrl, token);
            if (result.StatusCode != HttpStatusCode.Created)
            {
                microcksResource.Logger?.LogError("Failed to import remote artifact from '{RemoteUrl}' with status code {StatusCode}", remoteUrl, result.StatusCode);
                throw new InvalidOperationException($"Failed to import remote artifact from '{remoteUrl}'");
            }
            microcksResource.Logger?.LogInformation("Remote artifact imported successfully from '{RemoteUrl}'", remoteUrl);
        }
    }
}
