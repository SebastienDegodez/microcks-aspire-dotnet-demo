using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace Aspire.Hosting.Microcks.Clients;

public interface IImportArtifactClient
{
    [Multipart]
    [Headers("Accept: application/json")]
    [Post("/api/import")]
    Task<HttpResponseMessage> ImportArtifactAsync(
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken = default);
}
