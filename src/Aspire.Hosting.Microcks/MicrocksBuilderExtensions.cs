using System;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Microcks.MainArtifacts;
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

        builder.Eventing.Subscribe<ResourceReadyEvent>(microcksResource, (e, token) =>
            MicrocksResourceEventHandler.OnResourceReadyAsync(microcksResource, e, token));

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

}
