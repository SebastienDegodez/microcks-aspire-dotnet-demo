using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.Microcks.Clients.Model;

namespace Aspire.Hosting.Microcks.Clients;

/// <summary>
/// Provider interface for Microcks operations, encapsulating HttpClient functionality.
/// This interface provides high-level methods for interacting with Microcks services
/// including endpoint testing and message retrieval.
/// </summary>
public interface IMicrocksProvider
{
    /// <summary>
    /// Tests an endpoint using Microcks contract testing capabilities.
    /// </summary>
    /// <param name="testRequest">The test request containing all necessary test parameters.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the test result with validation outcomes.</returns>
    Task<TestResult> TestEndpointAsync(TestRequest testRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves messages (request/response pairs) for a specific test case within a test result.
    /// </summary>
    /// <param name="testResult">The test result containing the test case.</param>
    /// <param name="operationName">The operation name associated with the test case.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of request/response pairs for the specified test case.</returns>
    Task<List<RequestResponsePair>> GetMessagesForTestCaseAsync(
        TestResult testResult,
        string operationName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if the Microcks service is healthy and ready to accept requests.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the service is determined to be healthy.</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Uploads artifacts to the Microcks instance.
    /// </summary>
    /// <param name="artifactPath">The path to the artifact to upload.</param>
    /// <param name="isMainArtifact">Indicates if the artifact is a main artifact.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the upload operation.</returns>
    Task ImportArtifactAsync(string artifactPath, bool isMainArtifact, CancellationToken cancellationToken);

    /// <summary>
    /// Imports snapshots to the Microcks instance.
    /// </summary>
    /// <param name="artifactPath">The path to the snapshot artifact.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the import operation.</returns>
    Task ImportSnapshotAsync(string artifactPath, CancellationToken cancellationToken);

    /// <summary>
    /// Imports remote artifact to the Microcks instance.
    /// </summary>
    /// <param name="remoteUrl">The URL of the remote artifact to import.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the import operation.</returns>
    Task ImportRemoteArtifactAsync(string remoteUrl, CancellationToken cancellationToken);
}