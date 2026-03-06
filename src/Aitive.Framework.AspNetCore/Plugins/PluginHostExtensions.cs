using Aitive.Framework.Configuration.Plugins;
using Aitive.Framework.Patterns.Disposal;
using Aitive.Framework.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.AspNetCore.Plugins;

public static class PluginHostExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public IAsyncDisposable BindPlugins(IPluginHost pluginHost)
        {
            var initialServices = new Dictionary<Type, object>()
            {
                { typeof(IConfiguration), builder.Configuration },
                { typeof(IWebHostEnvironment), builder.Environment },
                { typeof(ILoggingBuilder), builder.Logging },
                { typeof(IMetricsBuilder), builder.Metrics },
                { typeof(WebApplicationBuilder), builder },
            };

            var serviceBindPoint = builder.Services.BindPluginServices(pluginHost, initialServices);
            var webRootBindPoint = builder.Environment.BindPluginWebRoot(pluginHost);
            var configurationBindPoint = builder.Services.BindPluginConfigurationOptions(
                pluginHost,
                builder.Configuration
            );

            return new MultiAsyncDisposable([
                webRootBindPoint,
                configurationBindPoint,
                serviceBindPoint,
            ]);
        }
    }

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
