using System;
using System.IO;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Microcks.FileArtifacts;
using Aspire.Hosting.Microcks.MainRemoteArtifacts;

namespace Aspire.Hosting;


/// <summary>
/// Extension methods to configure a Microcks resource on a distributed
/// application builder.
/// </summary>
public static class MicrocksBuilderExtensions
{
    /// <summary>
    /// Adds a Microcks resource to the distributed application and configures
    /// default HTTP endpoint, container image and registry.
    /// </summary>
    /// <param name="builder">The distributed application builder to extend.</param>
    /// <param name="name">The logical name of the Microcks resource. Must not be null or empty.</param>
    /// <returns>An <see cref="IResourceBuilder{MicrocksResource}"/> to further configure the resource.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
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
    /// Adds one or more main artifact file annotations to the Microcks resource.
    /// These artifacts will be uploaded to Microcks as primary artifacts when
    /// the resource is started.
    /// </summary>
    /// <param name="builder">The resource builder for the Microcks resource.</param>
    /// <param name="artifactFilePaths">File paths to the main artifact files to upload.</param>
    /// <returns>The same <see cref="IResourceBuilder{MicrocksResource}"/> instance for chaining.</returns>
    public static IResourceBuilder<MicrocksResource> WithMainArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] artifactFilePaths)
    {
        foreach (var sourcePath in artifactFilePaths)
        {
            string sourceFilePath = builder.ResolveFilePath(sourcePath);
            builder.WithAnnotation(new MainArtifactAnnotation(sourceFilePath));
        }

        return builder;
    }

    /// <summary>
    /// Adds remote artifact annotations (URLs) to be imported as main artifacts
    /// by the Microcks resource. These are useful to reference artifacts hosted
    /// externally (HTTP/HTTPS) instead of embedding files in the test resources.
    /// </summary>
    /// <param name="builder">The resource builder for the Microcks resource.</param>
    /// <param name="remoteArtifactUrls">Remote URLs pointing to artifact definitions.</param>
    /// <returns>The same <see cref="IResourceBuilder{MicrocksResource}"/> instance for chaining.</returns>
    public static IResourceBuilder<MicrocksResource> WithMainRemoteArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] remoteArtifactUrls)
    {
        foreach (var remoteArtifactUrl in remoteArtifactUrls)
        {
            builder.WithAnnotation(new MainRemoteArtifactAnnotation(remoteArtifactUrl));
        }

        return builder;
    }

    /// <summary>
    /// Adds one or more secondary artifact file annotations to the Microcks
    /// resource. Secondary artifacts may contain supplementary data (for
    /// example Postman collections) that complement main artifacts.
    /// </summary>
    /// <param name="builder">The resource builder for the Microcks resource.</param>
    /// <param name="artifactFilePaths">File paths to the secondary artifact files to upload.</param>
    /// <returns>The same <see cref="IResourceBuilder{MicrocksResource}"/> instance for chaining.</returns>
    public static IResourceBuilder<MicrocksResource> WithSecondaryArtifacts(this IResourceBuilder<MicrocksResource> builder, params string[] artifactFilePaths)
    {
        foreach (var sourcePath in artifactFilePaths)
        {
            string artifactFilePath = builder.ResolveFilePath(sourcePath);
            builder.WithAnnotation(new SecondaryArtifactAnnotation(artifactFilePath));
        }

        return builder;
    }

    /// <summary>
    /// Adds a snapshots annotation referencing a snapshots JSON file to the
    /// Microcks resource. Snapshots allow pre-populating Microcks with a
    /// previously exported repository state.
    /// </summary>
    /// <param name="builder">The resource builder for the Microcks resource.</param>
    /// <param name="snapshotsFilePath">The file path to the snapshots JSON file. Must not be null or whitespace.</param>
    /// <returns>The same <see cref="IResourceBuilder{MicrocksResource}"/> instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="snapshotsFilePath"/> is null or whitespace.</exception>
    public static IResourceBuilder<MicrocksResource> WithSnapshots(this IResourceBuilder<MicrocksResource> builder, string snapshotsFilePath)
    {
        if (string.IsNullOrWhiteSpace(snapshotsFilePath))
        {
            throw new ArgumentException("Snapshots file path cannot be null or whitespace.", nameof(snapshotsFilePath));
        }
        var resolvedPath = builder.ResolveFilePath(snapshotsFilePath);
        
        builder.WithAnnotation(new SnapshotsAnnotation(resolvedPath));
        return builder;
    }

    /// <summary>
    /// Resolves a file path, making it absolute if it is relative
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="sourcePath"></param>
    /// <returns></returns>
    private static string ResolveFilePath(this IResourceBuilder<MicrocksResource> builder, string sourcePath)
    {
        // If the source is a rooted path, use it directly without resolution
        return Path.IsPathRooted(sourcePath)
            ? sourcePath
            : Path.GetFullPath(sourcePath, builder.ApplicationBuilder.AppHostDirectory);
    }
}
