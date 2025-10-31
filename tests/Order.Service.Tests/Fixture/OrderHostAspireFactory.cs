
using Aspire.Hosting;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Testing;
using Projects;

namespace Order.IntegrationTests.Api;

public class OrderHostAspireFactory : IAsyncLifetime 
{
    /// <summary>
    /// CollectionName for ICollectionFixture
    /// </summary>
    public const string CollectionName = "Microcks Aspire Collection";

    public MicrocksResource MicrocksResource { get; private set; }

    /// <summary>
    /// The distributed application under test.
    /// </summary>
    public DistributedApplication App { get; private set; } = default!;

    public async ValueTask DisposeAsync()
    {
        await this.App.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await this.InitializeDistributedApplication();
    }

    private async Task InitializeDistributedApplication()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Order_AppHost>(TestContext.Current.CancellationToken);

        this.MicrocksResource = (MicrocksResource)builder.Resources.Single(r => r.Name == "microcks");
        
        this.App = await builder.BuildAsync(TestContext.Current.CancellationToken);
        
        await this.App.StartAsync()
            .ConfigureAwait(true);
    }
}