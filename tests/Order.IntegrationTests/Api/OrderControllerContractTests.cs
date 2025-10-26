using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Microcks.Clients.Model;
using Aspire.Hosting.Testing;
using Json.More;
using Projects;

namespace Order.IntegrationTests.Api;

public class OrderControllerContractTests
{
    [Fact]
    public async Task OrderControllerContractTest()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Order_AppHost>();
        
        await using var app = await builder.BuildAsync();

        await app.StartAsync();

        var projectResource = builder.Resources
            .OfType<ProjectResource>()
            .First();
        // DefaultContainerHostName
        // Get resource microcks
        var microcksResource = builder.Resources
            .OfType<MicrocksResource>()
            .First();
    
        var orderHttpClient = app.CreateHttpClient("Order-Api");
        // Act
        TestRequest request = new()
        {
            ServiceId = "Order Service API:0.1.0",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://host.docker.internal:8001", // Service DNS and target port
            // FilteredOperations can be used to limit the operations to test
        };

        // TODO: Migrate to Xunit.V3 
        var testResult = await microcksResource.TestEndpointAsync(request, CancellationToken.None);
        // Assert
        Assert.True(testResult.Success);
    }
}