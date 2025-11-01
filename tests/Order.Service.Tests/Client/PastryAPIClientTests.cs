//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0 
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Order.IntegrationTests.Api;
using Order.Service.Client;
using Order.Service.Client.Model;
using Xunit;

namespace Order.Service.Tests.Client;

[Collection(OrderHostAspireFactory.CollectionName)]
public class PastryAPIClientTests : IAsyncLifetime
{
    private readonly OrderHostAspireFactory orderHostAspireFactory;
    private readonly ITestOutputHelper testOutputHelper;

    public WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

    public PastryAPIClientTests(
        OrderHostAspireFactory orderHostAspireFactory,
        ITestOutputHelper testOutputHelper)
    {
        this.orderHostAspireFactory = orderHostAspireFactory;
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TestPastryAPIClient_ListPastriesAsync()
    {
        // Arrange
        DistributedApplication app = orderHostAspireFactory.App;
        var microcksProvider = app.CreateMicrocksProvider("microcks");

        var pastryAPIClient = this.WebApplicationFactory
            .Services
            .GetRequiredService<PastryAPIClient>(); // Ensure the client is registered

        List<Pastry> pastries = await pastryAPIClient.ListPastriesAsync("S", TestContext.Current.CancellationToken);
        Assert.Single(pastries); // Assuming there is 1 pastry in the mock data

        pastries = await pastryAPIClient.ListPastriesAsync("M", TestContext.Current.CancellationToken);
        Assert.Equal(2, pastries.Count); // Assuming there are 2 pastries in the mock

        pastries = await pastryAPIClient.ListPastriesAsync("L", TestContext.Current.CancellationToken);
        Assert.Equal(2, pastries.Count); // Assuming there is 1 pastry in the mock

        bool isVerified = await microcksProvider.VerifyAsync(
            "API Pastries", "0.0.1", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(isVerified, "Pastry API should be verified successfully");
    }

    [Fact]
    public async Task TestPastryAPIClient_GetPastryByNameAsync()
    {
        // Arrange
        DistributedApplication app = orderHostAspireFactory.App;
        var microcksProvider = app.CreateMicrocksProvider("microcks");
        double initialInvocationCount = await microcksProvider
            .GetServiceInvocationsCountAsync("API Pastries", "0.0.1", cancellationToken: TestContext.Current.CancellationToken);

        var pastryAPIClient = this.WebApplicationFactory
            .Services
            .GetRequiredService<PastryAPIClient>(); // Ensure the client is registered

        // Act & Assert : Millefeuille (disponible)
        var millefeuille = await pastryAPIClient.GetPastryByNameAsync("Millefeuille", TestContext.Current.CancellationToken);
        Assert.NotNull(millefeuille);
        Assert.Equal("Millefeuille", millefeuille.Name);
        Assert.True(millefeuille.IsAvailable());

        // Act & Assert : Éclair au café (disponible)
        var eclairCafe = await pastryAPIClient.GetPastryByNameAsync("Eclair Cafe", TestContext.Current.CancellationToken);
        Assert.NotNull(eclairCafe);
        Assert.Equal("Eclair Cafe", eclairCafe.Name);
        Assert.True(eclairCafe.IsAvailable());

        // Act & Assert : Éclair chocolat (indisponible)
        var eclairChocolat = await pastryAPIClient.GetPastryByNameAsync("Eclair Chocolat", TestContext.Current.CancellationToken);
        Assert.NotNull(eclairChocolat);
        Assert.Equal("Eclair Chocolat", eclairChocolat.Name);
        Assert.False(eclairChocolat.IsAvailable());

        // Vérifier le nombre d'invocations
        double finalInvocationCount = await microcksProvider.GetServiceInvocationsCountAsync("API Pastries", "0.0.1", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(initialInvocationCount + 3, finalInvocationCount);
    }

    public ValueTask InitializeAsync()
    {
        // Get Microcks Pastry API mock endpoint
        DistributedApplication app = orderHostAspireFactory.App;

        var microcksProvider = app.CreateMicrocksProvider("microcks");

        var pastryApiUrl = orderHostAspireFactory.MicrocksResource
            .GetRestMockEndpoint("API Pastries", "0.0.1")
            .ToString();
        // Add services for web/integration tests.
        this.WebApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test"); // Set environment to Test
                builder.UseSetting("PastryApi:BaseUrl", pastryApiUrl);
            });
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        WebApplicationFactory?.DisposeAsync();
        await Task.CompletedTask;
    }
}
