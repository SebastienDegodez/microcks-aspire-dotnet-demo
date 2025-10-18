using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks;
using Xunit;

namespace Aspire.Microcks.Testing.Fixtures;

/// <summary>
/// Shared fixture that starts a single Microcks instance for all tests in the collection.
/// Use this as a collection fixture so tests reuse the same running Microcks.
/// The fixture constructs a <see cref="TestDistributedApplicationBuilder"/>,
/// configures Microcks with the artifacts used by tests and starts the
/// distributed application once for the collection lifetime.
/// </summary>
public abstract class SharedMicrocksFixture : IAsyncLifetime, IDisposable
{
    public TestDistributedApplicationBuilder Builder { get; private set; } = default!;
    public DistributedApplication App { get; private set; } = default!;
    public MicrocksResource MicrocksResource { get; private set; } = default!;
    
    // Derived fixtures can override this to customize the builder (for example
    // to add additional container resources used by tests).
    protected virtual void ConfigureBuilder(TestDistributedApplicationBuilder builder)
    {
        // Default: no-op. Subclasses may add resources or adjust options.
    }

    /// <summary>
    /// Initializes the shared distributed application and starts Microcks.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Create builder without per-test ITestOutputHelper to avoid recreating logging per test
        Builder = TestDistributedApplicationBuilder.Create();

        // Allow derived fixtures to customize the builder before adding Microcks
        ConfigureBuilder(Builder);

        // Configure Microcks with the artifacts used by tests so services are available
        var microcksBuilder = Builder.AddMicrocks("microcks")
            .WithSnapshots(Path.Combine("resources", "microcks-repository.json"))
            .WithMainArtifacts(
                Path.Combine("resources", "apipastries-openapi.yaml"),
                Path.Combine("resources", "subdir", "weather-forecast-openapi.yaml")
            )
            .WithSecondaryArtifacts(
                Path.Combine("resources", "apipastries-postman-collection.json")
            )
            .WithMainRemoteArtifacts("https://raw.githubusercontent.com/microcks/microcks/master/samples/APIPastry-openapi.yaml");

        App = Builder.Build();
        await App.StartAsync(CancellationToken.None).ConfigureAwait(false);

        MicrocksResource = microcksBuilder.Resource;
    }

    /// <summary>
    /// Stops the distributed application and disposes the builder.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (App is not null)
            {
                await App.StopAsync(CancellationToken.None).ConfigureAwait(false);
                App.Dispose();
            }
        }
        catch
        {
            // swallow, we're tearing down tests
        }

        Builder?.Dispose();
    }

    public void Dispose()
    {
        _ = DisposeAsync();
    }
}
