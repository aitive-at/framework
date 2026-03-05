using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.AspNetCore.Plugins;

public sealed class WebRootPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly string _webRootPath;

    public WebRootPluginBindPointBuilder(
        IWebHostEnvironment hostEnvironment,
        string webRootPath = "wwwroot"
    )
    {
        _hostEnvironment = hostEnvironment;
        _webRootPath = webRootPath;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        var oldWebRootFileProvider = _hostEnvironment.WebRootFileProvider;

        var fileProviders = new List<IFileProvider>();

        foreach (var plugin in plugins)
        {
            fileProviders.AddRange(
                plugin
                    .Query<IPluginFileProviderFactory>()
                    .Select(p => p.Create(_webRootPath, _hostEnvironment.IsDevelopment()))
            );
        }

        fileProviders.Add(oldWebRootFileProvider.Trace("DefaultWebRoot"));

        _hostEnvironment.WebRootFileProvider = new CompositeFileProvider(fileProviders);

        return new WebRootPluginBindPoint(_hostEnvironment, oldWebRootFileProvider);
    }
}
