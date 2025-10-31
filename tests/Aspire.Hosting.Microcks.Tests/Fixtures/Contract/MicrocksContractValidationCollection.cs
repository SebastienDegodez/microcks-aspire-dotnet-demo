using Aspire.Microcks.Testing.Fixtures.Contract;
using Xunit;

namespace Aspire.Microcks.Testing.Features.Mocking.Contract;

/// <summary>
/// Collection definition used to share the <see cref="MicrocksContractValidationFixture"/>
/// between tests. Tests that depend on a running Microcks instance should
/// belong to this collection.
/// </summary>
[CollectionDefinition(MicrocksContractValidationCollection.CollectionName)]
public class MicrocksContractValidationCollection : ICollectionFixture<MicrocksContractValidationFixture>
{
    // Collection definition for sharing Microcks with Bad-Implementation and Good-Implementation resources.
    public const string CollectionName = "Microcks contract collection";
}
