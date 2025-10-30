
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Projects;

namespace Order.IntegrationTests.Api;

public class MicrocksAspireFactory : IAsyncLifetime 
{
    /// <summary>
    /// CollectionName for ICollectionFixture
    /// </summary>
    public const string CollectionName = "Microcks Aspire Collection";
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
            .CreateAsync<Order_AppHost>( TestContext.Current.CancellationToken);

        this.App = await builder.BuildAsync(TestContext.Current.CancellationToken);

        await this.App.StartAsync()
            .ConfigureAwait(true);
    }
}