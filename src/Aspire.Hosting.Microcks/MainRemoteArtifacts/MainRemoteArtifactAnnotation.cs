
using System;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Microcks.MainRemoteArtifacts;

internal sealed class MainRemoteArtifactAnnotation : IResourceAnnotation
{
    public string RemoteArtifactUrl { get; }

    public MainRemoteArtifactAnnotation(string remoteArtifactUrl)
    {
        ArgumentNullException.ThrowIfNull(remoteArtifactUrl, nameof(remoteArtifactUrl));
        RemoteArtifactUrl = remoteArtifactUrl;
    }
}