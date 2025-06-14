using Refit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aspire.Hosting.Microcks.Clients;

/// <summary>
/// Defines the endpoint for instructing Microcks to download an artifact from a remote URL.
/// </summary>
public interface IDownloadArtifactClient
{
    /// <summary>
    /// Instructs Microcks to download an artifact from a remote URL and import it.
    /// </summary>
    /// <param name="mainArtifact">Indicates if the artifact is the main one.</param>
    /// <param name="url">The remote URL of the artifact.</param>
    /// <returns>The HTTP response from Microcks.</returns>
    [Post("/api/artifact/download")]
    [Headers("Accept: application/json")]
    Task<HttpResponseMessage> DownloadArtifactAsync(
        [Query] bool mainArtifact,
        [Query] string url,
        CancellationToken cancellationToken = default);
}