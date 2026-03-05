using Microsoft.Extensions.FileProviders;

namespace Aitive.Framework.Plugins;

public interface IPluginFileProviderFactory
{
    IFileProvider Create(string? rootPath = null, bool isInDevelopmentMode = false);
}
