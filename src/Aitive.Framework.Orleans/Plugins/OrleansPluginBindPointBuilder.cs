using System.Reflection;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Aitive.Framework.Orleans.Plugins;

public sealed class OrleansPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IServiceCollection _services;

    public OrleansPluginBindPointBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        _services.AddSerializer(options =>
        {
            foreach (var assembly in plugins.SelectMany(p => p.Query<Assembly>()))
            {
                options.AddAssembly(assembly);
            }
        });

        return new EmptyPluginBindPoint();
    }
}
