using Xunit;

namespace Aspire.Microcks.Testing.Fixtures.Mock;

/// <summary>
/// Collection definition used to share the <see cref="SharedMicrocksFixture"/>
/// between tests. Tests that depend on a running Microcks instance should
/// belong to this collection.
/// </summary>
[CollectionDefinition(MicrocksMockingCollection.CollectionName)]
public class MicrocksMockingCollection : ICollectionFixture<MicrocksMockingFixture>
{
    // Collection definition for sharing a single Microcks instance across tests
    public const string CollectionName = "Microcks mocking collection";
}
