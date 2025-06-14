using Aspire.Hosting.ApplicationModel;
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
    public static async Task ImportAsync(MicrocksResource resource, CancellationToken token)
    {
        var remoteArtifactUrls = resource.Annotations.OfType<MainRemoteArtifactAnnotation>();
        if (remoteArtifactUrls == null || !remoteArtifactUrls.Any())
            return;

        var endpoint = resource.GetEndpoint("http");
        var hostUrl = endpoint.Url;
        var client = RestService.For<IDownloadArtifactEndpoint>(hostUrl);

        foreach (var remoteUrl in remoteArtifactUrls.Select(a => a.RemoteArtifactUrl))
        {
            var result = await client.DownloadArtifactAsync(true, remoteUrl, token);
            if (result.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException($"Failed to import remote artifact from '{remoteUrl}'");
        }
    }
}
