using Aitive.Framework.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.AspNetCore.Plugins;

internal sealed class WebRootPluginBindPoint : IPluginBindPoint
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IFileProvider _oldFileProvider;

    internal WebRootPluginBindPoint(
        IWebHostEnvironment hostEnvironment,
        IFileProvider oldFileProvider
    )
    {
        _hostEnvironment = hostEnvironment;
        _oldFileProvider = oldFileProvider;
    }

    public ValueTask DisposeAsync()
    {
        _hostEnvironment.WebRootFileProvider = _oldFileProvider;
        return ValueTask.CompletedTask;
    }
}
