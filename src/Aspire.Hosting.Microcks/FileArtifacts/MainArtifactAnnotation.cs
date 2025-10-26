
using System;
using System.IO;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Microcks.FileArtifacts;

internal sealed class MainArtifactAnnotation : IResourceAnnotation
{
    public string SourcePath { get; }

    public MainArtifactAnnotation(string sourcePath)
    {
        ArgumentNullException.ThrowIfNull(sourcePath, nameof(sourcePath));
        
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Artifact file not found: {sourcePath}");
        }

        SourcePath = sourcePath;
    }
}
