using System.Reflection;
using Aitive.Framework.Functional;
using Aitive.Framework.Plugins.Tracing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Plugins.Providers;

internal sealed class ProjectPluginFileProviderFactory : IPluginFileProviderFactory
{
    private readonly PluginManifest _pluginManifest;
    private readonly Assembly _assembly;

    internal ProjectPluginFileProviderFactory(PluginManifest pluginManifest, Assembly assembly)
    {
        _pluginManifest = pluginManifest;
        _assembly = assembly;
    }

    public IFileProvider Create(string? rootPath = null, bool isInDevelopmentMode = false)
    {
        var innerProviders = new List<IFileProvider>();
        var rootNamespace = $"{_assembly.GetName()?.Name}";

        if (rootPath != null)
        {
            rootNamespace += $".{rootPath}";
        }

        innerProviders.Add(
            new EmbeddedFileProvider(_assembly, rootNamespace).Trace(
                CreateId("Embedded", rootNamespace)
            )
        );

        return new CompositeFileProvider(innerProviders);
    }

    private string CreateId(string type, string? rootPath)
    {
        var actualRootPath = rootPath ?? "NoRootPath";

        return $"{type}_{_pluginManifest.VersionId}_{actualRootPath}";
    }

    private static Optional<string> GetProjectRootFromAssembly(Assembly asm)
    {
        var projectDirectory = new DirectoryInfo(System.IO.Path.GetDirectoryName(asm.Location)!);

        // Walk up until we find a *.csproj (dev) or give up.
        while (projectDirectory is { Exists: true })
        {
            if (projectDirectory.GetFiles("*.csproj").Length > 0)
            {
                return projectDirectory.FullName;
            }

            projectDirectory = projectDirectory.Parent;
        }

        return Optional.None<string>();
    }
}
