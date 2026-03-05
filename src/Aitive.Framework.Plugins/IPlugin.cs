namespace Aitive.Framework.Plugins;

public interface IPlugin
{
    PluginId Id => Manifest.Id;

    PluginVersionId VersionId => Manifest.VersionId;

    PluginManifest Manifest { get; }

    /// <summary>
    /// Queries for a given interface as implemented by the plugin.
    /// </summary>
    /// <param name="interfaceType">The type of interface to request.</param>
    /// <param name="services">External services that can be consumed during service instantiation.</param>
    /// <returns>
    /// All implementations of that interface.
    /// If this is disposable or async disposable it is the responsibilty of the caller to dispose the returned instances.</returns>
    IEnumerable<object> Query(Type interfaceType, IReadOnlyDictionary<Type, object> services);
}

public static class PluginExtensions
{
    extension(IPlugin plugin)
    {
        public IEnumerable<T> Query<T>()
        {
            return plugin.Query<T>(new Dictionary<Type, object>());
        }

        public IEnumerable<T> Query<T>(IReadOnlyDictionary<Type, object> services)
        {
            return plugin.Query(typeof(T), services).Cast<T>();
        }
    }
}
