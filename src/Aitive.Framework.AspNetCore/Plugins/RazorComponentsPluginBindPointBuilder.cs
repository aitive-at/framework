using System.Reflection;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;
using Microsoft.AspNetCore.Builder;

namespace Aitive.Framework.AspNetCore.Plugins;

public sealed class RazorComponentsPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly RazorComponentsEndpointConventionBuilder _endpointConventionBuilder;

    public RazorComponentsPluginBindPointBuilder(
        RazorComponentsEndpointConventionBuilder endpointConventionBuilder
    )
    {
        _endpointConventionBuilder = endpointConventionBuilder;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        var assemblies = plugins.SelectMany(p => p.Query<Assembly>());

        _endpointConventionBuilder.AddAdditionalAssemblies(assemblies.ToArray());

        return new EmptyPluginBindPoint();
    }
}
