using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Plugins.Binding;

public sealed class ServiceBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IServiceCollection _services;

    public ServiceBindPointBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins) { }
}
