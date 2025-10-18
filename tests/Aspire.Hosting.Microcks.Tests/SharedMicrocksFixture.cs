using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Microcks;
using Xunit;

namespace Aspire.Microcks.Testing;

/// <summary>
/// Shared fixture that starts a single Microcks instance for all tests in the collection.
/// Use this as a collection fixture so tests reuse the same running Microcks.
/// The fixture constructs a <see cref="TestDistributedApplicationBuilder"/>,
/// configures Microcks with the artifacts used by tests and starts the
/// distributed application once for the collection lifetime.
/// </summary>
public sealed class SharedMicrocksFixture : IAsyncLifetime, IDisposable
{
    public TestDistributedApplicationBuilder Builder { get; private set; } = default!;
    public DistributedApplication App { get; private set; } = default!;
    public MicrocksResource MicrocksResource { get; private set; } = default!;

    /// <summary>
    /// Initializes the shared distributed application and starts Microcks.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Create builder without per-test ITestOutputHelper to avoid recreating logging per test
        Builder = TestDistributedApplicationBuilder.Create();

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
