using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins;

public record PluginRequest(PluginId Id, SemVersionRange VersionRange)
{
    public PluginRequest(PluginId Id)
        : this(Id, SemVersionRange.All) { }

    public static implicit operator PluginRequest(string value) => new(new PluginId(value));
}
