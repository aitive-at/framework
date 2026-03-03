namespace Aitive.Framework.Plugins;

public interface IPlugin
{
    PluginId Id => Manifest.Id;
    PluginManifest Manifest { get; }

    /// <summary>
    /// Queries for a given interface as implemented by the plugin.
    /// </summary>
    /// <param name="interfaceType">The type of interface to request.</param>
    /// <returns>
    /// All implementations of that interface.
    /// If this is disposable or async disposable it is the responsibilty of the caller to dispose the returned instances.</returns>
    IEnumerable<object> Query(Type interfaceType);
}
