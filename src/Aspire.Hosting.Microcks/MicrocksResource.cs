using System;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks.Clients;
using Microsoft.Extensions.Logging;
using Refit;

namespace Aspire.Hosting.Microcks;

/// <summary>
/// Represents a Microcks container resource within an Aspire distributed application.
/// This class provides methods to retrieve mock endpoints for different service protocols, including SOAP, REST, GraphQL, and gRPC.
/// It is intended for use in service discovery and integration testing scenarios.
/// </summary>
public class MicrocksResource(string name) : ContainerResource(name)
{
    internal const string PrimaryEndpointName = "http";

    public ILogger<MicrocksResource> Logger { get; private set; }

    internal Lazy<IMicrocksClient> Client => new(CreateMicrocksClient);

    private IMicrocksClient CreateMicrocksClient()
    {
        var client = RestService.For<IMicrocksClient>(GetEndpoint().Url);
        return client;
    }

    /// <summary>
    /// Gets an endpoint reference.
    /// </summary>
    /// <returns>An <see cref="EndpointReference"/> object representing the endpoint reference.</returns>
    public EndpointReference GetEndpoint()
    {
        return new EndpointReference(this, PrimaryEndpointName);
    }

    /// <summary>
    /// Returns the SOAP mock endpoint for a given service name and version.
    /// </summary>
    /// <param name="serviceName">The name of the target service.</param>
    /// <param name="serviceVersion">The version of the target service.</param>
    /// <returns>The URI of the corresponding SOAP mock endpoint.</returns>
    public Uri GetSoapMockEndpoint(string serviceName, string serviceVersion)
    {
        var httpEndpoint = this.GetEndpoint();
        return new UriBuilder(httpEndpoint.Url)
        {
            Path = $"soap/{serviceName}/{serviceVersion}"
        }.Uri;
    }

    /// <summary>
    /// Returns the REST mock endpoint for a given service name and version.
    /// </summary>
    /// <param name="serviceName">The name of the target service.</param>
    /// <param name="serviceVersion">The version of the target service.</param>
    /// <returns>The URI of the corresponding REST mock endpoint.</returns>
    public Uri GetRestMockEndpoint(string serviceName, string serviceVersion)
    {
        var httpEndpoint = this.GetEndpoint();
        return new UriBuilder(httpEndpoint.Url)
        {
            Path = $"rest/{serviceName}/{serviceVersion}"
        }.Uri;
    }

    /// <summary>
    /// Returns the GraphQL mock endpoint for a given service name and version.
    /// </summary>
    /// <param name="serviceName">The name of the target service.</param>
    /// <param name="serviceVersion">The version of the target service.</param>
    /// <returns>The URI of the corresponding GraphQL mock endpoint.</returns>
    public Uri GetGraphQLMockEndpoint(string serviceName, string serviceVersion)
    {
        var httpEndpoint = this.GetEndpoint();
        return new UriBuilder(httpEndpoint.Url)
        {
            Path = $"graphql/{serviceName}/{serviceVersion}"
        }.Uri;
    }

    /// <summary>
    /// Returns the gRPC mock endpoint for the Microcks resource.
    /// </summary>
    /// <returns>The URI of the corresponding Grpc mock endpoint.</returns>
    public Uri GetGrpcMockEndpoint()
    {
        var httpEndpoint = this.GetEndpoint();

        return new UriBuilder(httpEndpoint.Url)
        {
            Scheme = "grpc"
        }.Uri;
    }

    internal void SetLogger(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        Logger = loggerFactory.CreateLogger<MicrocksResource>();
    }

    internal async Task WaitForHealthAsync(CancellationToken cancellationToken)
    {
        this.Logger?.LogInformation("Waiting for Microcks resource '{ResourceName}' to be healthy", this.Name);

        await TryWaitForHealthAsync(cancellationToken);
    }

    /// <summary>
    /// Tries to wait for the health of the Microcks resource.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task TryWaitForHealthAsync(CancellationToken cancellationToken)
    {
        const int retryCount = 3;
        var retryDelay = TimeSpan.FromMilliseconds(100);
        
        for (var i = 0; i < retryCount; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Logger?.LogInformation("Health check cancelled for Microcks resource '{ResourceName}'", Name);

                return;
            }

            if (await IsHealthyAsync(cancellationToken, i + 1))
            {
                return;
            }

            await Task.Delay(retryDelay, cancellationToken);
        }

        Logger?.LogInformation("Microcks resource '{ResourceName}' is unhealthy", Name);
    }

    private async Task<bool> IsHealthyAsync(CancellationToken cancellationToken, int attempt)
    {
        try
        {
            var healthCheckResponse = await Client.Value
                .CheckHealthAsync(cancellationToken)
                .ConfigureAwait(false);

            if (healthCheckResponse.IsSuccessStatusCode)
            {
                Logger?.LogInformation("Microcks resource '{ResourceName}' is healthy", Name);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Exception during health check for Microcks resource '{ResourceName}', attempt {Attempt}", Name, attempt);
        }

        return false;
    }

}