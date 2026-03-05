using Aitive.Framework.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Plugins;

public static class PluginHostExtensions
{
    extension(IPluginHost host)
    {
        public IPluginBindPoint BindPluginWebRoot(
            IWebHostEnvironment hostingEnvironment,
            string webRootPath = "wwwroot"
        )
        {
            var builder = new WebRootPluginBindPointBuilder(hostingEnvironment, webRootPath);
            return host.Bind(builder);
        }

        public IPluginBindPoint BindPluginControllersWithViews(IMvcBuilder mvcBuilder)
        {
            var builder = new ApplicationPartsPluginBindPointBuilder(mvcBuilder);
            return host.Bind(builder);
        }

        public IPluginBindPoint BindPluginRazorComponents(
            RazorComponentsEndpointConventionBuilder conventionsBuilder
        )
        {
            var builder = new RazorComponentsPluginBindPointBuilder(conventionsBuilder);
            return host.Bind(builder);
        }
    }

    extension(IMvcBuilder mvcBuilder)
    {
        public IPluginBindPoint BindPluginControllersWithViews(IPluginHost pluginHost)
        {
            return pluginHost.BindPluginControllersWithViews(mvcBuilder);
        }
    }

    extension(IWebHostEnvironment hostEnvironment)
    {
        public IPluginBindPoint BindPluginWebRoot(
            IPluginHost pluginHost,
            string webRootPath = "wwwroot"
        )
        {
            return pluginHost.BindPluginWebRoot(hostEnvironment, webRootPath);
        }
    }

    extension(RazorComponentsEndpointConventionBuilder conventionBuilder)
    {
        public IPluginBindPoint BindPluginRazorComponents(IPluginHost pluginHost)
        {
            return pluginHost.BindPluginRazorComponents(conventionBuilder);
        }
    }
}
