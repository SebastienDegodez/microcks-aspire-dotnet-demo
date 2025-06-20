---
applyTo: 'tests/**/*.cs'
---
# Rules for Unit and Integration Tests

This rule defines the best practices and tools to use for writing unit and integration tests in the backend project.

## Libraries and Tools
- **Test Framework**: Always use `xunit.v3` for unit and integration tests.
- **Mocks**: Use `FakeItEasy` to create mocks in unit tests.
- **Contract Tests or Advanced Mocks**: Use `Testcontainers` and `Microcks` to simulate SOAP or REST calls, or for contract tests.
- **Integration Tests**: 
  - For **Aspire-based projects**, use Aspire's `DistributedApplicationFactory` and Aspire resources for integration tests. Do **not** use `Testcontainers` for resources supported by Aspire.
  - For **classic ASP.NET Core projects**, use `Testcontainers` to manage external dependencies (databases, APIs, etc.).
  - If a required resource is not supported by Aspire, you may use `Testcontainers` as a fallback, even in Aspire-based projects.
  - See `.github/instructions/integration-test-factories.dotnet.instructions.md` for detailed guidance and code examples.

## General Steps
1. **Unit Tests**:
   - Write tests for each valid and invalid scenario.
   - Use `FakeItEasy` to mock dependencies.
   - Verify exceptions and expected results.

2. **Integration Tests**:
   - For Aspire-based projects, set up the test environment using Aspire's orchestration and `DistributedApplicationFactory`.
   - For classic projects, set up a test environment with `Testcontainers`.
   - Simulate SOAP or REST calls with `Microcks` if necessary.
   - Validate the entire business flow.

3. **Performance Tests**:
   - Add tests to validate the performance of critical features.

## Unit Tests
- Located in `tests/[project].UnitTests/`.
- Test only the **Domain** and **Application** layers.
- Use `FakeItEasy` to mock dependencies.
- Do not interact with real databases or external services.
- Focus on business logic, use cases, and validation.
- Example folders: `UseCases/`, `Services/`.

## Integration Tests
- Located in `tests/[project].IntegrationTests/`.
- Test the **Infrastructure** and **Api** layers, and the integration between layers.
- For Aspire-based projects, use Aspire's orchestration and `DistributedApplicationFactory` to set up real or simulated external dependencies.
- For classic projects, use `Testcontainers` to set up real or simulated external dependencies (databases, APIs, etc.).
- Use `Microcks` for contract or advanced integration tests (SOAP, REST, events).
- Validate the entire business flow, including data persistence and external calls.
- Example folders: `Features/` (for API endpoint tests).

## Best Practices
- Always write tests before implementation (TDD).
- Document test cases in the corresponding files.
- Use explicit test names to describe their purpose.

## Additional Rule: Continuous Test Execution

- After every significant change or addition, run `dotnet test` or `dotnet watch` to verify the current state of the project.
- This ensures that all tests pass and helps identify issues early in the development process.
- Document the results of the test runs in the corresponding task or user story.

## Updates
This rule must be updated if new tools or practices are adopted in the backend project.