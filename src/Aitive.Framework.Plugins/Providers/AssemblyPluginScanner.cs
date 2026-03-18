using System.Reflection;
using System.Runtime.Loader;
using Aitive.Framework.Functional;
using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins.Providers;

internal static class AssemblyPluginScanner
{
    private static readonly SemVersion _defaultVersion = new SemVersion(1, 0, 0);
    private static readonly SemVersionRange _defaultVersionRange = SemVersionRange.All;

    internal static Optional<PluginManifest> Scan(Assembly assembly)
    {
        var pluginAttribute = assembly.GetCustomAttribute<PluginAttribute>();

        if (pluginAttribute == null)
        {
            return Optional.None<PluginManifest>();
        }

        if (!SemVersion.TryParse(pluginAttribute.Version, out var version))
        {
            version = _defaultVersion;
        }

        var dependencyAttributes = assembly.GetCustomAttributes<PluginDependencyAttribute>();
        var dependencies = new List<PluginDependency>();

        foreach (var dependencyAttribute in dependencyAttributes)
        {
            if (
                dependencyAttribute.VersionRange == null
                || !SemVersionRange.TryParse(dependencyAttribute.VersionRange, out var versionRange)
            )
            {
                versionRange = _defaultVersionRange;
            }

            dependencies.Add(new PluginDependency(dependencyAttribute.Id, versionRange));
        }

        var propertyAttributes = assembly.GetCustomAttributes<PluginPropertyAttribute>();

        var properties = new Dictionary<string, object>();

        foreach (var propertyAttribute in propertyAttributes)
        {
            properties[propertyAttribute.Key] = propertyAttribute.Value;
        }

        return new PluginManifest(
            pluginAttribute.Id,
            pluginAttribute.Description,
            version,
            dependencies,
            properties
        );
    }
}
