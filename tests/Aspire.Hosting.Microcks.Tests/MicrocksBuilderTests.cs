using Xunit;

using FakeItEasy;

using System;
using System.Linq;

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Microcks.Tests;

/// <summary>
/// Tests that validate the behavior of the <c>AddMicrocks</c> builder extension
/// without starting any external resources. These tests run fast and only
/// exercise builder configuration logic.
/// </summary>
public class MicrocksBuilderTests
{
    /// <summary>
    /// Ensures that passing null or whitespace to the builder extension
    /// <c>AddMicrocks</c> results in an <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="name">Input name to validate.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMicrocks_WithNullOrWhitespaceName_ShouldThrowsException(string name)
    {
        IDistributedApplicationBuilder builder = A.Fake<IDistributedApplicationBuilder>();

        Assert.Throws<ArgumentException>(() => builder.AddMicrocks(name!));
    }

    /// <summary>
    /// Verifies that calling <c>AddMicrocks</c> with valid arguments registers
    /// a resource and sets its default container image and registry annotations.
    /// </summary>
    [Fact]
    public void AddMicrocks_WithValidParameters_ShouldConfigureResourceCorrectly()
    {
        var builder = DistributedApplication.CreateBuilder();

        var name = $"microcks{Guid.NewGuid()}";
        var microcks = builder.AddMicrocks(name);

        Assert.NotNull(microcks.Resource);
        Assert.Equal(name, microcks.Resource.Name);

        var containerImageAnnotation = microcks.Resource
            .Annotations
            .OfType<ContainerImageAnnotation>()
            .FirstOrDefault();

        Assert.Equal(MicrocksContainerImageTags.Image, containerImageAnnotation?.Image);
        Assert.Equal(MicrocksContainerImageTags.Tag, containerImageAnnotation?.Tag);
        Assert.Equal(MicrocksContainerImageTags.Registry, containerImageAnnotation?.Registry);
    }
}
