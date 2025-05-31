---
applyTo: 'tests/**/*.IntegrationTests/**/*.cs'
---
# Factory Selection for Integration Tests (.NET)

When writing integration tests, always choose the factory that matches your application type:

- **Use `WebApplicationFactory<TEntryPoint>`** (from `Microsoft.AspNetCore.Mvc.Testing`) for classic ASP.NET Core applications:
  - Applies when testing a single web API or service that does **not** use .NET Aspire or distributed application features.
  - Example: Minimal API, MVC, or classic ASP.NET Core projects.

- **Use `DistributedApplicationFactory`** (from `Aspire.Hosting.Testing`) for .NET Aspire-based distributed applications:
  - Applies when your AppHost/Program.cs uses `DistributedApplication.CreateBuilder` and you orchestrate multiple services/components.
  - This is the preferred approach for Aspire-based microservices, distributed topologies, or when you want to test the full distributed environment.
  - Example usage:
    ```csharp
    using Aspire.Hosting.Testing;
    using Xunit;

    public class MyAspireIntegrationTests : IClassFixture<DistributedApplicationFactory>
    {
        private readonly DistributedApplicationFactory _factory;
        public MyAspireIntegrationTests(DistributedApplicationFactory factory) => _factory = factory;
        [Fact]
        public async Task Test() {
            var client = _factory.CreateHttpClient("Order-Api");
            // ...
        }
    }
    ```

**Summary Table:**
| Scenario                                 | Factory to Use                  |
|-------------------------------------------|---------------------------------|
| Classic ASP.NET Core API                  | WebApplicationFactory           |
| .NET Aspire distributed application       | DistributedApplicationFactory   |

> Always prefer `DistributedApplicationFactory` for Aspire-based projects. Use `WebApplicationFactory` only for non-Aspire, single-service APIs.
