

using System;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.Microcks.Clients.Model;
using Refit;

namespace Aspire.Hosting.Microcks.Clients;

public interface IMetricsInvocationClient
{
    /// <summary>
    /// Gets the service invocations count for a specific service and version on a given day.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="serviceVersion">Version of the service.</param>
    /// <param name="day">The day for which to retrieve the invocation count (format: yyyyMMdd)</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Daily invocation statistics.</returns>
    [Get("/api/metrics/invocations/{serviceName}/{serviceVersion}")]
    Task<ApiResponse<DailyInvocationStatistic>> GetServiceInvocationsCountAsync(
        string serviceName,
        string serviceVersion,
        [Query(Format = "yyyyMMdd")] DateOnly? day,
        CancellationToken cancellationToken);
}