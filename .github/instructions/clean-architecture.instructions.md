---
applyTo: '**/*.cs'
---

# Clean Architecture

When implementing backend services, follow these Clean Architecture principles to ensure maintainability, scalability, and separation of concerns.

## Structure

1. Organize the project into the following layers:
   - **Domain**: Contains core business logic, entities, value objects, and domain events.
   - **Application**: Contains use cases, commands, queries, and interfaces for external services.
   - **Infrastructure**: Contains implementations for external services, database access, and third-party integrations.
   - **Api**: Contains API endpoints, controllers, and request/response models.

2. Each layer should only depend on the layers below it:
   - **Domain** has no dependencies.
   - **Application** depends only on **Domain**.
   - **Infrastructure** depends on **Application** and **Domain**.
   - **Api** depends on **Application** and **Domain**.

## Implementation

1. **Domain Layer**:
   - Define entities with business rules and validation logic.
   - Use value objects for immutable data types.
   - Raise domain events for significant business actions.

2. **Application Layer**:
   - Define services for handling commands and queries (e.g., `IUserService`, `IOrderService`).
   - Implement commands and queries as methods in these services.
   - Define interfaces for external dependencies (e.g., repositories, services).
   - Implement validation logic using FluentValidation.

3. **Infrastructure Layer**:
   - Implement interfaces defined in the **Application** layer.
   - Use dependency injection to register implementations.
   - Keep database access logic in repositories.

4. **Api Layer**:
   - Use API endpoints to handle HTTP requests and responses.
   - Map request models to service methods and response models to API responses.
   - Avoid business logic in this layer.


## Feature-based Structure (Recommended)

**Prefer a feature-oriented (domain-driven) folder structure over a technical one.**

- Each feature (e.g., Order, Customer, etc.) has its own folder in each layer (Domain, Application, Infrastructure, Api, tests).
- Technical files (repository, service, etc.) are grouped by feature, not by type.
- This approach improves navigation and business understanding.

### Example of feature-oriented folder structure

```
src/
    [project].Domain/
        Order/
            Order.cs
            OrderLine.cs
            OrderCreatedEvent.cs
        Customer/
            Customer.cs
    [project].Application/
        Order/
            IOrderService.cs
            CreateOrderCommand.cs
            CreateOrderValidator.cs
        Customer/
    [project].Infrastructure/
        Order/
            OrderRepository.cs
            OrderKafkaPublisher.cs
        Customer/
    [project].Api/
        Order/
            OrderController.cs
        Customer/
tests/
    [project].UnitTests/ # Unit test project for Domain and Application layers
        Order/
            OrderTests.cs
        Customer/
    [project].IntegrationTests/ # Integration test project for Infrastructure, Api layers, and architecture validation
        Order/
            OrderIntegrationTests.cs
        Customer/
```

> Replace technical folders (Entities, ValueObjects, Services, etc.) with business/feature folders (Order, Customer, etc.) in each layer.

## Testing Guidelines

1. **Unit Tests**:
   - Located in `tests/[project].UnitTests/`.
   - Test the **Domain** and **Application** layers.
   - Focus on business logic, use cases, and validation.

2. **Integration Tests**:
   - Located in `tests/[project].IntegrationTests/`.
   - Test the **Infrastructure** and **Api** layers.
   - Validate database interactions, external service integrations, API endpoints, and adherence to Clean Architecture principles.
   - Use [ArchUnit.NET](https://github.com/TNG/ArchUnitNET) to ensure:
     - **Domain** has no dependencies on other layers.
     - **Application** depends only on **Domain**.
     - **Infrastructure** depends only on **Application** and **Domain**.
     - **Api** depends only on **Application** and **Domain**.

## Additional Guidelines

1. Use dependency injection to manage dependencies across layers.
2. Avoid circular dependencies between layers.
3. Write unit tests for **Domain** and **Application** layers.
4. Use integration tests for **Infrastructure** and **Api** layers.
5. Follow SOLID principles within each layer.
6. Avoid using a mediator library; instead, directly call service methods from the **Api** layer.

# References
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/TheCleanArchitecture.html)
