using Projects;

namespace Order.IntegrationTests.Api;

[CollectionDefinition(MicrocksAspireFactory.CollectionName)]
public class AspireCollection : ICollectionFixture<MicrocksAspireFactory>
{
}