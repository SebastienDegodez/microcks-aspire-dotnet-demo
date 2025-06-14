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
public class MicrocksResourceTests(ITestOutputHelper testOutputHelper)
{

    /// <summary>
    /// Verifies that calling <c>AddMicrocks</c> with a null or whitespace name
    /// throws an <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="name">The invalid name to pass to <c>AddMicrocks</c>.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]    
    public void AddMicrocks_WithNullOrWhitespaceName_ShouldThrowsException(string name)
    {
        IDistributedApplicationBuilder builder = A.Fake<IDistributedApplicationBuilder>();

        Assert.Throws<ArgumentException>(() => builder.AddMicrocks(name!));
    }

    /// <summary>
    /// Verifies that adding a Microcks resource with valid parameters configures
    /// the resource and its container image annotation correctly.
    /// </summary>
    [Fact]
    public void AddMicrocks_WithValidParameters_ShouldConfigureResourceCorrectly()
    {
        var builder = DistributedApplication.CreateBuilder();

        var name = $"microcks{Guid.NewGuid()}";
        var microcks = builder.AddMicrocks(name);

        Assert.NotNull(microcks.Resource);
        Assert.Equal(name, microcks.Resource.Name);

        var containerImageAnnotation = microcks.Resource
            .Annotations
            .OfType<ContainerImageAnnotation>()
            .FirstOrDefault();

        Assert.Equal(MicrocksContainerImageTags.Image, containerImageAnnotation?.Image);
        Assert.Equal(MicrocksContainerImageTags.Tag, containerImageAnnotation?.Tag);
        Assert.Equal(MicrocksContainerImageTags.Registry, containerImageAnnotation?.Registry);
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
        using var builder = TestDistributedApplicationBuilder.Create(testOutputHelper);

        var microcks = builder.AddMicrocks("microcks");

        await using var app = await builder.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(microcks.Resource);

        var endpoint = microcks.Resource.GetEndpoint("http");
        Assert.NotNull(endpoint);

        Uri baseSoapEndpoint = microcks.Resource.GetSoapMockEndpoint("Pastries Service", "1.0");
        Assert.Equal($"{endpoint.Url}/soap/Pastries Service/1.0", baseSoapEndpoint.ToString());

        Uri baseRestEndpoint = microcks.Resource.GetRestMockEndpoint("Pastries Service", "0.0.1");
        Assert.Equal($"{endpoint.Url}/rest/Pastries Service/0.0.1", baseRestEndpoint.ToString());

        Uri baseGraphQLEndpoint = microcks.Resource.GetGraphQLMockEndpoint("Pastries Graph", "1");
        Assert.Equal($"{endpoint.Url}/graphql/Pastries Graph/1", baseGraphQLEndpoint.ToString());

        Uri baseGrpcEndpoint = microcks.Resource.GetGrpcMockEndpoint();

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
        using var builder = TestDistributedApplicationBuilder.Create(testOutputHelper);

        var microcks = builder.AddMicrocks("microcks")
            .WithSnapshots(Path.Combine("resources", "microcks-repository.json"))
            .WithMainArtifacts(
                Path.Combine("resources", "apipastries-openapi.yaml"),
                Path.Combine("resources", "subdir", "weather-forecast-openapi.yaml")
            )
            .WithSecondaryArtifacts(
                Path.Combine("resources", "apipastries-postman-collection.json")
            )
            .WithMainRemoteArtifacts("https://raw.githubusercontent.com/microcks/microcks/master/samples/APIPastry-openapi.yaml");

        await using var app = await builder.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(microcks.Resource);

        var endpoint = microcks.Resource.GetEndpoint("http");
        Assert.NotNull(endpoint);
        var uriBuilder = new UriBuilder(endpoint.Url)
        {
            Path = "api/services"
        };

        // Call uri to get the list of services
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