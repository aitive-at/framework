namespace Aitive.Framework.Plugins;

public interface IPlugin
{
    PluginId Id => Description.Id;
    PluginDescription Description { get; }

    IEnumerable<object> Query(Type interfaceType);
}
