using Refit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aspire.Hosting.Microcks.Clients;

/// <summary>Upload an artifact</summary>
public interface IUploadArtifactClient
{
    [Multipart]
    [Post("/api/artifact/upload?mainArtifact={mainArtifact}")]
    [Headers("Accept: application/json")]
    Task<HttpResponseMessage> UploadArtifactAsync(
        [Query] bool mainArtifact,
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken = default);
}
