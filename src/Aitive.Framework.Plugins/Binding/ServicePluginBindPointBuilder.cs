using Aitive.Framework.Collections;
using Aitive.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Plugins.Binding;

public sealed class ServicePluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IServiceCollection _services;

    public ServicePluginBindPointBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        var serviceModules = plugins
            .SelectMany(p => p.Query<IServiceModule>())
            .PossiblyOrdered()
            .ToList();

        foreach (var serviceModule in serviceModules)
        {
            serviceModule.Register(_services);
        }

        return new EmptyPluginBindPoint();
    }
}
