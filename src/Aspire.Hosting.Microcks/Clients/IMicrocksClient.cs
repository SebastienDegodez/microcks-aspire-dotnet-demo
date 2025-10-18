namespace Aspire.Hosting.Microcks.Clients;

public interface IMicrocksClient :
    IUploadArtifactClient,
    IDownloadArtifactClient,
    IImportArtifactClient,
    IHealthCheckClient,
    ITestsEndpointClient
{
    // This interface aggregates all the clients needed for Microcks operations.
    // It can be extended with more clients as needed.
}
