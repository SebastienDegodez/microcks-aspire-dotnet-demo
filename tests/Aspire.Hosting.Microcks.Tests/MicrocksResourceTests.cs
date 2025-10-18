using Xunit;

using FakeItEasy;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aspire.Hosting.ApplicationModel;
using Aspire.Microcks.Testing;
using Aspire.Hosting.Testing;

namespace Aspire.Hosting.Microcks.Tests;

/// <summary>
/// Tests for the Microcks resource builder and runtime behavior.
/// </summary>
/// <remarks>
/// These tests verify that the Microcks resource is configured correctly when
/// added to a distributed application builder, that its endpoints and mock
/// service URI helpers are available at runtime, and that imported artifacts
/// expose the expected services list.
/// </remarks>
[Collection("Microcks collection")]
public class MicrocksResourceTests : IClassFixture<SharedMicrocksFixture>
{
    private readonly SharedMicrocksFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public MicrocksResourceTests(SharedMicrocksFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Builds a test distributed application with Microcks and ensures that
    /// the mock endpoints helpers return the expected URIs for SOAP, REST,
    /// GraphQL and gRPC mocks when the application is started.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddMicrocks_ShouldConfigureMockEndpoints()
    {
    // Use the shared Microcks instance started by the collection fixture
    var microcks = _fixture.MicrocksResource;

    await Task.CompletedTask; // placeholder to keep async signature; resource already started by fixture

    Assert.NotNull(microcks);

    var endpoint = microcks.GetEndpoint();
        Assert.NotNull(endpoint);

    Uri baseSoapEndpoint = microcks.GetSoapMockEndpoint("Pastries Service", "1.0");
        Assert.Equal($"{endpoint.Url}/soap/Pastries Service/1.0", baseSoapEndpoint.ToString());

    Uri baseRestEndpoint = microcks.GetRestMockEndpoint("Pastries Service", "0.0.1");
        Assert.Equal($"{endpoint.Url}/rest/Pastries Service/0.0.1", baseRestEndpoint.ToString());

    Uri baseGraphQLEndpoint = microcks.GetGraphQLMockEndpoint("Pastries Graph", "1");
        Assert.Equal($"{endpoint.Url}/graphql/Pastries Graph/1", baseGraphQLEndpoint.ToString());

    Uri baseGrpcEndpoint = microcks.GetGrpcMockEndpoint();

        var uriBuilder = new UriBuilder(endpoint.Url)
        {
            Scheme = "grpc"
        };
        Assert.Equal(uriBuilder.Uri, baseGrpcEndpoint);
    }
    
    /// <summary>
    /// Verifies that when snapshots and artifacts are provided to the Microcks
    /// builder, the running Microcks instance exposes the expected services
    /// list via its <c>/api/services</c> endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]    
    public async Task AddMicrocks_WithArtifacts_ShouldAvailableServices()
    {
        // Use the shared Microcks instance started by the collection fixture
        var microcks = _fixture.MicrocksResource;
        var app = _fixture.App;

        Assert.NotNull(microcks);

        var endpoint = microcks.GetEndpoint();
        Assert.NotNull(endpoint);
        var uriBuilder = new UriBuilder(endpoint.Url)
        {
            Path = "api/services"
        };

        // Call uri to get the list of services using the shared app's http client
        using var httpClient = app.CreateHttpClient("microcks");
        var response = await httpClient.GetAsync(uriBuilder.Uri, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var servicesJson = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(servicesJson);

        Assert.Equal(7, jsonDoc.RootElement.GetArrayLength());

        var expectedNames = new[]
        {
            "Petstore API",
            "HelloService Mock",
            "io.github.microcks.grpc.hello.v1.HelloService",
            "Movie Graph API",
            "API Pastry - 2.0",
            "API Pastries",
            "WeatherForecast API"
        };

        // Extract the names from the JSON
        var actualNames = jsonDoc.RootElement
            .EnumerateArray()
            .Select(e => e.GetProperty("name").GetString())
            .ToList();
        // Assert that all expected names are present in the actual names
        foreach (var expectedName in expectedNames)
        {
            Assert.Contains(actualNames, name => name == expectedName);
        }
    }

}