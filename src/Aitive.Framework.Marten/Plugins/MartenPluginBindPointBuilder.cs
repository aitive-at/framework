using System.Reflection;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;
using Marten;

namespace Aitive.Framework.Marten.Plugins;

public sealed class MartenPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly StoreOptions _storeOptions;

    public MartenPluginBindPointBuilder(StoreOptions storeOptions)
    {
        _storeOptions = storeOptions;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        _storeOptions.AutoRegister(scanner =>
        {
            foreach (var assembly in plugins.SelectMany(p => p.Query<Assembly>()))
            {
                scanner.Assembly(assembly);
            }
        });

        return new EmptyPluginBindPoint();
    }
}
