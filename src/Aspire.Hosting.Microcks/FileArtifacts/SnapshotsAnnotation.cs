using System;
using System.IO;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Microcks.FileArtifacts;

internal sealed class SnapshotsAnnotation : IResourceAnnotation
{
    public string SnapshotsFilePath { get; }

    public SnapshotsAnnotation(string snapshotsFilePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(snapshotsFilePath, nameof(snapshotsFilePath));
        if (!File.Exists(snapshotsFilePath))
        {
            throw new FileNotFoundException($"Snapshots file not found: {snapshotsFilePath}", snapshotsFilePath);
        }
        SnapshotsFilePath = snapshotsFilePath;
    }
}
