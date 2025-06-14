using Refit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aspire.Hosting.Microcks.MainArtifacts;

/// <summary>Upload an artifact</summary>
public interface IUploadArtifactEndpoint
{
    [Multipart()]
    [Post("/api/artifact/upload")]
    Task<HttpResponseMessage> UploadArtifactAsync(
        [Query] bool mainArtifact,
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken = default);
}
