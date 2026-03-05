using System.Reflection;
using Aitive.Framework.Functional;
using Microsoft.Extensions.FileProviders;

namespace Aitive.Framework.Plugins.Providers;

public class ProjectPlugin : AssemblyPlugin
{
    public ProjectPlugin(PluginManifest manifest, Assembly assembly)
        : base(manifest, assembly) { }

    protected override IEnumerable<object> QueryCore(
        Type interfaceType,
        IReadOnlyDictionary<Type, object> services
    )
    {
        if (interfaceType == typeof(IPluginFileProviderFactory))
        {
            var overrides = base.QueryCore(interfaceType, services).ToList();

            if (overrides.Any())
            {
                return overrides;
            }

            return [new ProjectPluginFileProviderFactory(Manifest, Assembly)];
        }

        return base.QueryCore(interfaceType, services);
    }
}
