using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Microcks.MainArtifacts;
using Aspire.Hosting.Microcks.MainRemoteArtifacts;

namespace Aspire.Hosting.Microcks;

/// <summary>
/// Handles resource events for Microcks orchestration.
/// </summary>
internal static class MicrocksResourceEventHandler
{
    /// <summary>
    /// Handles the event when the Microcks resource is ready.
    /// </summary>
    /// <param name="resource">The Microcks resource that is ready.</param>
    /// <param name="resourceReadyEvent">The resource event.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task OnResourceReadyAsync(
        MicrocksResource resource,
        ResourceReadyEvent resourceReadyEvent,
        CancellationToken cancellationToken = default)
    {
        await MicrocksArtifactUploader.UploadAsync(resource, cancellationToken);
        await MicrocksRemoteArtifactImporter.ImportAsync(resource, cancellationToken);
    }
}
