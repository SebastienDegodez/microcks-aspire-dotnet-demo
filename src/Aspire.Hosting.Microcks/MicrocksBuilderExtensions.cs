using System;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Microcks.FileArtifacts;
using Aspire.Hosting.Microcks.MainRemoteArtifacts;
namespace Aspire.Hosting;

public static class MicrocksBuilderExtensions
{
    /// <summary>
    /// Adds a Microcks resource to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the Microcks resource.</param>
    /// <returns>The resource builder for the Microcks resource.</returns>
    public static IResourceBuilder<MicrocksResource> AddMicrocks(this IDistributedApplicationBuilder builder, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        var microcksResource = new MicrocksResource(name);
        var resourceBuilder = builder
            .AddResource(microcksResource)
            .WithHttpEndpoint(targetPort: 8080, name: MicrocksResource.PrimaryEndpointName)
            .WithImage(MicrocksContainerImageTags.Image, MicrocksContainerImageTags.Tag)
            .WithImageRegistry(MicrocksContainerImageTags.Registry);

        builder.Services.TryAddLifecycleHook<MicrocksResourceLifecycleHook>();

        return resourceBuilder;
    }

    /// <summary>
    /// Specifies the main artifact files to be uploaded to the Microcks resource.
    /// </summary>
    /// <param name="builder">The Microcks resource to configure.</param>
    /// <param name="artifactFilePaths">A collection of file paths for the main artifacts to upload.</param>
    /// <returns>The configured Microcks resource.</returns>
    public static IResourceBuilder<MicrocksResource> WithMainArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] artifactFilePaths)
    {
        foreach (var sourcePath in artifactFilePaths)
        {
            builder.WithAnnotation(new MainArtifactAnnotation(sourcePath));
        }

        return builder;
    }

    /// <summary>
    /// Specifies the remote artifact URLs to be imported as main artifacts in the Microcks resource.
    /// </summary>
    /// <param name="builder">The Microcks resource to configure.</param>
    /// <param name="remoteArtifactUrls">A collection of remote artifact URLs to import.</param>
    /// <returns>The configured Microcks resource.</returns>
    public static IResourceBuilder<MicrocksResource> WithMainRemoteArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] remoteArtifactUrls)
    {
        foreach (var remoteArtifactUrl in remoteArtifactUrls)
        {
            builder.WithAnnotation(new MainRemoteArtifactAnnotation(remoteArtifactUrl));
        }

        return builder;
    }

    /// <summary>
    /// Specifies the secondary artifact files to be uploaded to the Microcks resource.
    /// </summary>
    /// <param name="builder">The Microcks resource to configure.</param>
    /// <param name="artifactFilePaths">A collection of file paths for the secondary artifacts to upload.</param>
    /// <returns>The configured Microcks resource.</returns>
    public static IResourceBuilder<MicrocksResource> WithSecondaryArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] artifactFilePaths)
    {
        foreach (var artifactFilePath in artifactFilePaths)
        {
            builder.WithAnnotation(new SecondaryArtifactAnnotation(artifactFilePath));
        }

        return builder;
    }

    /// <summary>
    /// Specifies the snapshots file to be uploaded to the Microcks resource.
    /// </summary>
    /// <param name="builder">The Microcks resource to configure.</param>
    /// <param name="snapshotsFilePath">The file path of the snapshots to upload.</param>
    /// <returns>The configured Microcks resource.</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshots file path is null or whitespace.</exception>
    public static IResourceBuilder<MicrocksResource> WithSnapshots(this IResourceBuilder<MicrocksResource> builder, string snapshotsFilePath)
    {
        if (string.IsNullOrWhiteSpace(snapshotsFilePath))
        {
            throw new ArgumentException("Snapshots file path cannot be null or whitespace.", nameof(snapshotsFilePath));
        }

        builder.WithAnnotation(new SnapshotsAnnotation(snapshotsFilePath));
        return builder;
    }
}
