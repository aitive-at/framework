using Semver;

namespace Aitive.Framework.Plugins;

public sealed record PluginDescription(PluginId Id, SemVersion Version) { }
