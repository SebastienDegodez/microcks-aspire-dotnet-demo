# Domain-Driven Design (DDD) Guidelines

## Purpose
These rules help you apply Domain-Driven Design principles to ensure your codebase is expressive, maintainable, and aligned with business needs.

## Core Principles
- Focus on the core domain and domain logic.
- Use the Ubiquitous Language: model your code and terminology after the business language.
- Collaborate closely with domain experts.

## Key Concepts
- **Entity**: An object defined by its identity, not just its attributes.
- **Value Object**: An immutable object that is defined only by its attributes.
- **Aggregate**: A cluster of domain objects treated as a single unit. Each aggregate has a root and a boundary.
- **Aggregate Root**: The main entity that controls access to the aggregate.
- **Domain Event**: An event that represents something that happened in the domain.
- **Repository**: A mechanism for encapsulating storage, retrieval, and search behavior.
- **Service**: Domain logic that doesn’t naturally fit within an entity or value object.

## Best Practices
- Model your aggregates and entities to reflect real business concepts.
- Keep aggregates small and focused.
- Use value objects to encapsulate concepts with no identity.
- Raise domain events for significant business actions.
- Avoid using CRUD verbs (Create, Update, Delete, Get) in domain method names—prefer business-intent names (e.g., PlaceOrder, ActivateAccount).
- Encapsulate invariants and business rules within aggregates.
- Use repositories only for aggregate roots.
- Keep domain logic out of application and infrastructure layers.

## Example Structure
```
src/
  [project].Domain/
    Order/
      Order.cs
      OrderLine.cs
      OrderPlacedEvent.cs
    Customer/
      Customer.cs
      Address.cs
```

## Implementation Checklist
- [ ] Use the Ubiquitous Language in code and documentation
- [ ] Model aggregates and value objects
- [ ] Encapsulate business rules in domain objects
- [ ] Use domain events for important changes
- [ ] Avoid anemic domain models (put logic in domain objects, not just data)