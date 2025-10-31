namespace Aspire.Hosting.Microcks.Clients;

public interface IMicrocksClient :
    IUploadArtifactClient,
    IDownloadArtifactClient,
    IImportArtifactClient,
    IHealthCheckClient,
    ITestsEndpointClient,
    IMetricsInvocationClient
{
    // This interface aggregates all the clients needed for Microcks operations.
    // It can be extended with more clients as needed.
}
