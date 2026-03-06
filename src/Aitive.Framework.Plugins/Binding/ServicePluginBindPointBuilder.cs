using Aitive.Framework.Collections;
using Aitive.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Plugins.Binding;

public sealed class ServicePluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IServiceCollection _services;
    private readonly IReadOnlyDictionary<Type, object> _initialServices;

    public ServicePluginBindPointBuilder(
        IServiceCollection services,
        IReadOnlyDictionary<Type, object> initialServices
    )
    {
        _services = services;
        _initialServices = initialServices;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        var serviceModules = plugins
            .SelectMany(p => p.Query<IServiceModule>(_initialServices))
            .PossiblyOrdered()
            .ToList();

        foreach (var serviceModule in serviceModules)
        {
            serviceModule.Register(_services);
        }

        return new EmptyPluginBindPoint();
    }
}
