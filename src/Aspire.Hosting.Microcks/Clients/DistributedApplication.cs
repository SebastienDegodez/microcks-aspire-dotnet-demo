using System;
using Aspire.Hosting.Microcks.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Aspire.Hosting;

/// <summary>
/// Extension methods for working with Microcks in a distributed application.
/// </summary>
public static class DistributedApplicationExtensions
{
    /// <summary>
    /// Creates an instance of IMicrocksProvider using the application's service provider.
    /// </summary>
    /// <param name="app">The distributed application instance.</param>
    /// <param name="resourceName">The name of the Microcks resource to use.</param>
    /// <returns>An instance of IMicrocksProvider.</returns>
    /// <exception cref="ArgumentNullException">Thrown if app is null.</exception>
    public static IMicrocksProvider CreateMicrocksProvider(this DistributedApplication app, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resourceName);

        // Create a scope to resolve scoped services
        using var scope = app.Services.CreateScope();
        var microcksProvider = scope.ServiceProvider.GetRequiredKeyedService<IMicrocksProvider>(resourceName);

        return microcksProvider;
    }
}