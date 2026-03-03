using Semver;

namespace Aitive.Framework.Plugins;

public sealed record PluginDependency(PluginId Id, SemVersionRange VersionRange);

public sealed record PluginManifest(
    PluginId Id,
    string Description,
    SemVersion Version,
    IReadOnlyList<PluginDependency> Dependencies
) { }
