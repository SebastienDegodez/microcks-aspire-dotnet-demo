using Projects;

namespace Order.IntegrationTests.Api;

[CollectionDefinition(OrderHostAspireFactory.CollectionName)]
public class AspireCollection : ICollectionFixture<OrderHostAspireFactory>
{
}