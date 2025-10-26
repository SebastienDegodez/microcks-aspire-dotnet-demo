using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Microcks.Testing;
using Xunit;

namespace Aspire.Microcks.Testing.Fixtures.Mock;

/// <summary>
/// Example derived fixture that adds two container resources (bad/good implementations)
/// to the shared distributed application builder before Microcks is configured.
/// </summary>
public sealed class MicrocksMockingFixture : SharedMicrocksFixture
{   
}
