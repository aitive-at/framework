using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins;

public sealed record PluginDependency(PluginId Id, SemVersionRange VersionRange);

public sealed record PluginManifest(
    PluginId Id,
    string Description,
    SemVersion Version,
    IReadOnlyList<PluginDependency> Dependencies,
    IReadOnlyDictionary<string, object> Properties
);

public static class PluginManifestExtensions
{
    extension(PluginManifest manifest)
    {
        public PluginVersionId VersionId => new(manifest.Id, manifest.Version);
    }
}
