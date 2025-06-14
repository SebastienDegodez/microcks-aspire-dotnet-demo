using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace Aspire.Hosting.Microcks.Clients;

public interface IHealthCheckClient
{
    [Get("/api/health")]
    [Headers("Accept: application/json")]
    Task<HttpResponseMessage> CheckHealthAsync(CancellationToken cancellationToken);
}
