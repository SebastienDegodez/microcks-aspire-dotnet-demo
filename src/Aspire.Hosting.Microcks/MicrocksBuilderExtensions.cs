using System;
using System.IO;
using System.Net.Http;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.Microcks;
using Aspire.Hosting.Microcks.Clients;
using Aspire.Hosting.Microcks.FileArtifacts;
using Aspire.Hosting.Microcks.MainRemoteArtifacts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Refit;

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

        // Configure Refit to use System.Text.Json with explicit options so
        // enum values are serialized exactly as defined in the enum (no
        // naming policy that could change casing or underscores).
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            // Do not use a naming policy which could alter enum text
            PropertyNamingPolicy = null,
            // Ensure numbers are not used for enums
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };

        builder.Services.AddHttpClient(name, (sp, httpClient) =>
        {
            var endpointUrl = microcksResource.GetEndpoint().Url;
            var logger = sp.GetRequiredService<ILogger<MicrocksResource>>();
            logger.LogInformation("Configuring Microcks HttpClient for endpoint URL: {EndpointUrl}", endpointUrl);
            httpClient.BaseAddress = new Uri(endpointUrl);
        });

        // Explicitly register IMicrocksClient as a keyed service to ensure it's available for DI
        builder.Services.AddKeyedScoped(name, (serviceProvider, serviceKey) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(serviceKey.ToString());
            return RestService.For<IMicrocksClient>(httpClient, refitSettings);
        });

        // Register keyed service for MicrocksProvider
        builder.Services.AddKeyedScoped<IMicrocksProvider>(name, (serviceProvider, serviceKey) =>
        {
            var client = serviceProvider.GetRequiredKeyedService<IMicrocksClient>(serviceKey);
            var logger = serviceProvider.GetRequiredService<ILogger<MicrocksProvider>>();
            var provider = new MicrocksProvider(client, logger);

            return provider;
        });

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
