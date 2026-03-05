using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins;

public interface IPluginProvider
{
    IEnumerable<PluginManifest> AvailablePlugins { get; }

    IPlugin Load(PluginVersionId pluginVersionId);
}
