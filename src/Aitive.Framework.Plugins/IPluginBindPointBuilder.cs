namespace Aitive.Framework.Plugins;

public interface IPluginBindPointBuilder
{
    IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins);
}
