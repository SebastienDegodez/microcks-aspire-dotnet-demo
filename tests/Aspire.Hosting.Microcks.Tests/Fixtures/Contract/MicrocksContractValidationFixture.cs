using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Microcks.Testing.Fixtures.Contract;

/// <summary>
/// Example derived fixture that adds two container resources (bad/good implementations)
/// to the shared distributed application builder before Microcks is configured.
/// </summary>
public sealed class MicrocksContractValidationFixture : SharedMicrocksFixture
{
    private const string BAD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:01";
    private const string GOOD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:02";

    protected override void ConfigureBuilder(TestDistributedApplicationBuilder builder)
    {
        // Add bad implementation container
        var badImpl = new ContainerResource("bad-impl");
        builder.AddResource(badImpl)
            .WithImage(BAD_PASTRY_IMAGE)
            .WithHttpEndpoint(targetPort: 3001, name: "http");

        // Add good implementation container
        var goodImpl = new ContainerResource("good-impl");
        builder.AddResource(goodImpl)
            .WithImage(GOOD_PASTRY_IMAGE)
            .WithHttpEndpoint(targetPort: 3002, name: "http");
    }

}
