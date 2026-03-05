using System.Reflection;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Plugins;

public class ApplicationPartsPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IMvcBuilder _mvcBuilder;

    public ApplicationPartsPluginBindPointBuilder(IMvcBuilder mvcBuilder)
    {
        _mvcBuilder = mvcBuilder;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        _mvcBuilder.ConfigureApplicationPartManager(partsManager =>
        {
            foreach (var assembly in plugins.SelectMany(p => p.Query<Assembly>()))
            {
                partsManager.ApplicationParts.Add(new AssemblyPart(assembly));
                partsManager.ApplicationParts.Add(new CompiledRazorAssemblyPart(assembly));
            }
        });

        return new EmptyPluginBindPoint();
    }
}
