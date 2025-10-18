using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.Microcks.Clients.Model;
using Refit;

/// <summary>
/// Client interface for Microcks test endpoint operations.
/// </summary>
public interface ITestsEndpointClient
{
    /// <summary>
    /// Tests an endpoint based on the provided test request.
    /// </summary>
    /// <param name="testRequest">The test request containing the necessary information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    [Post("/api/tests")]
    Task<TestResult> TestEndpointAsync(TestRequest testRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the test result by its ID.
    /// </summary>
    /// <param name="testResultId">ID of the test result to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    [Get("/api/tests/{testResultId}")]
    Task<TestResult> RefreshTestResultAsync(string testResultId, CancellationToken cancellationToken);


    /// <summary>
    /// Retrieves messages for a specific test case within a test result.
    /// </summary>
    /// <param name="testResultId">The ID of the test result.</param>
    /// <param name="testCaseId">The ID of the test case.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of request/response pairs for the specified test case.</returns>
    [Get("/api/tests/{testResultId}/messages/{testCaseId}")]
    Task<List<RequestResponsePair>> GetMessagesForTestCaseAsync(string testResultId, string testCaseId, CancellationToken cancellationToken);
}