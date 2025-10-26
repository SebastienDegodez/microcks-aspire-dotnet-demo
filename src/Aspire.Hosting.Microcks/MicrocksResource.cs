using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks.Clients;
using Aspire.Hosting.Microcks.Clients.Model;
using Microsoft.Extensions.Logging;
using Refit;

namespace Aspire.Hosting.Microcks;

/// <summary>
/// Represents a Microcks container resource within an Aspire distributed application.
/// This class provides methods to retrieve mock endpoints for different service protocols, including SOAP, REST, GraphQL, and gRPC.
/// It is intended for use in service discovery and integration testing scenarios.
/// </summary>
public class MicrocksResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string PrimaryEndpointName = "http";

    public ILogger<MicrocksResource> Logger { get; private set; }

    internal Lazy<IMicrocksClient> Client => new(CreateMicrocksClient);

    private IMicrocksClient CreateMicrocksClient()
    {
        // Configure Refit to use System.Text.Json with explicit options so
        // enum values are serialized exactly as defined in the enum (no
        // naming policy that could change casing or underscores).
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            // Do not use a naming policy which could alter enum text
            PropertyNamingPolicy = null,
            // Ensure numbers are not used for enums
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };

        var endpointUrl = GetEndpoint().Url;

        Logger?.LogInformation("Creating Microcks client for endpoint URL: {EndpointUrl}", endpointUrl);
        var client = RestService.For<IMicrocksClient>(endpointUrl, refitSettings);

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


    /// <summary>
    /// Tests an endpoint using Microcks.
    /// </summary>
    /// <param name="testRequest">The test request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The test result.</returns>
    public async Task<TestResult> TestEndpointAsync(TestRequest testRequest, CancellationToken cancellationToken)
    {
        var client = this.Client.Value;

        TestResult testResult = await client.TestEndpointAsync(testRequest, cancellationToken)
            .ConfigureAwait(false);

        // Handle successful creation
        Logger?.LogInformation("Test request for service '{ServiceId}' completed successfully.", testRequest.ServiceId);

        var testResultId = testResult.Id;
        this.Logger?.LogDebug("Test Result ID: {TestResultId}, new polling for progression",
            testResultId);

        // Polling for test result completion
        try
        {
            await WaitForConditionAsync(async () => !(await client.RefreshTestResultAsync(testResultId, cancellationToken)).InProgress,
                atMost: TimeSpan.FromMilliseconds(1000).Add(testRequest.Timeout),
                delay: TimeSpan.FromMilliseconds(100),
                interval: TimeSpan.FromMilliseconds(200),
                cancellationToken);
        }
        catch (TaskCanceledException taskCanceledException)
        {
            this.Logger.LogWarning(
                taskCanceledException,
                "Test timeout reached, stopping polling for test {testEndpoint}", testRequest.TestEndpoint);
        }

        return await client.RefreshTestResultAsync(testResultId, cancellationToken);
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan atMost, TimeSpan delay, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        // Delay before first check
        await Task.Delay(delay, cancellationToken);

        // Cancel after atMost
        using var atMostCancellationToken = new CancellationTokenSource(atMost);
        // Create linked token so we can be cancelled either by caller or by timeout
        using var cancellationTokenSource = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, atMostCancellationToken.Token);

        // Polling
        while (!await condition())
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            await Task.Delay(interval, cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// Retrieves messages for a specific test case within a test result.
    /// </summary>
    /// <param name="testResult">The test result containing the test case.</param>
    /// <param name="operationName">The operation name associated with the test case.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of request/response pairs for the specified test case.</returns>
    public async Task<List<RequestResponsePair>> GetMessagesForTestCaseAsync(
        TestResult testResult,
        string operationName,
        CancellationToken cancellationToken)
    {
        var operation = operationName.Replace('/', '!');
        var testCaseId = $"{testResult.Id}-{testResult.TestNumber}-{HttpUtility.UrlEncode(operation)}";

        var client = this.Client.Value;
        return await client.GetMessagesForTestCaseAsync(testResult.Id, testCaseId, cancellationToken);
    }
}