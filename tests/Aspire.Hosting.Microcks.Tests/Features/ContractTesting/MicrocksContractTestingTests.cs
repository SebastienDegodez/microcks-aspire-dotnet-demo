using System;
using System.Threading.Tasks;

using Xunit;

using Aspire.Microcks.Testing.Fixtures.Contract;
using Aspire.Hosting.Microcks.Clients.Model;
using System.Collections.Generic;

namespace Aspire.Hosting.Microcks.Tests.Features.ContractTesting;

/// <summary>
/// Aspire-based rewrite of the contract-testing demo that previously used Testcontainers.
/// This test starts Microcks and two container resources (bad/good implementations) in a single
/// distributed application and performs basic verification: the Microcks services list is available
/// and the demo implementations expose HTTP endpoints.
/// </summary>
[Collection("Microcks contract collection")]
public sealed class MicrocksContractTestingTests : IClassFixture<MicrocksContractValidationFixture>
{
    private readonly MicrocksContractValidationFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public MicrocksContractTestingTests(MicrocksContractValidationFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Tests calling the TestEndpoint API of Microcks with the bad implementation,
    /// expecting validation failures.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task WhenCallingTestEndpoint_WithBadImplementation_ShouldReturnValidationFailures()
    {
        Assert.NotNull(_fixture.MicrocksResource);
        Assert.NotNull(_fixture.App);

        var microcksResource = _fixture.MicrocksResource;

        TestRequest badTestRequest = new()
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://bad-impl:3001",
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
        // Call TestEndpoint from Microcks Resource
        var badTestResult = await microcksResource.TestEndpointAsync(
            badTestRequest,
            TestContext.Current.CancellationToken);

        Assert.False(badTestResult.Success);
        Assert.Equal("http://bad-impl:3001", badTestRequest.TestEndpoint);
        Assert.Equal(3, badTestResult.TestCaseResults.Count);
        Assert.Contains("string found, number expected",
        badTestResult.TestCaseResults[0].TestStepResults[0].Message);

        List<RequestResponsePair> messages = await microcksResource.GetMessagesForTestCaseAsync(
            badTestResult,
            "GET /pastries",
            TestContext.Current.CancellationToken);

        Assert.Equal(3, messages.Count);
        Assert.All(messages, message =>
        {
            Assert.NotNull(message.Request);
            Assert.NotNull(message.Response);
            Assert.NotNull(message.Response.Content);
            // Check these are the correct requests.
            Assert.NotNull(message.Request.QueryParameters);
            Assert.Single(message.Request.QueryParameters);
            Assert.Equal("size", message.Request.QueryParameters[0].Name);
        });
    }


    /// <summary>
    /// Tests calling the TestEndpoint API of Microcks with the good implementation,
    /// expecting validation success.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task WhenCallingTestEndpoint_WithGoodImplementation_ShouldReturnValidationSuccess()
    {
        Assert.NotNull(_fixture.MicrocksResource);
        Assert.NotNull(_fixture.App);

        var microcksResource = _fixture.MicrocksResource;

        TestRequest goodTestRequest = new()
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://good-impl:3002",
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
        // Call TestEndpoint from Microcks Resource
        var goodTestResult = await microcksResource.TestEndpointAsync(
            goodTestRequest,
            TestContext.Current.CancellationToken);

        Assert.True(goodTestResult.Success);
        Assert.Equal("http://good-impl:3002", goodTestRequest.TestEndpoint);
        Assert.Equal(3, goodTestResult.TestCaseResults.Count);
        Assert.Empty(goodTestResult.TestCaseResults[0].TestStepResults[0].Message);
    }

}
