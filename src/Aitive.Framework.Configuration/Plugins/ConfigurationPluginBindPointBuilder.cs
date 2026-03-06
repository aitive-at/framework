using System.Reflection;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Configuration.Plugins;

public sealed class ConfigurationPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ConfigurationPluginBindPointBuilder(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        _services = services;
        _configuration = configuration;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        foreach (var assembly in plugins.SelectMany(p => p.Query<Assembly>()))
        {
            _services.AddConfigurationOptions(assembly, _configuration);
        }

        return new EmptyPluginBindPoint();
    }
}
