using System;
using System.IO;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Microcks.FileArtifacts;

/// <summary>
/// Represents a secondary artifact annotation.
/// </summary>
internal sealed class SecondaryArtifactAnnotation : IResourceAnnotation
{
    public string SourcePath { get; }

    public SecondaryArtifactAnnotation(string sourcePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sourcePath, nameof(sourcePath));
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Artifact file not found: {sourcePath}");
        }
        
        SourcePath = sourcePath;
    }
}
