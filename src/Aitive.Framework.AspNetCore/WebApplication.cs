using Aitive.Framework.Application;
using Aitive.Framework.AspNetCore.Plugins;
using Aitive.Framework.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore;

public abstract class WebApplication<TSelf>
    : Application<
        Microsoft.AspNetCore.Builder.WebApplicationBuilder,
        Microsoft.AspNetCore.Builder.WebApplication,
        TSelf
    >
    where TSelf : WebApplication<TSelf>
{
    protected WebApplication(ApplicationOptions options, IApplicationDescription description)
        : base(options, description) { }

    protected override WebApplicationBuilder OnCreateBuilder()
    {
        return WebApplication.CreateBuilder();
    }

    protected override void OnConfigureBuilder(WebApplicationBuilder builder)
    {
        builder.BindPlugins(PluginHost);
        builder.Services.AddMvc().BindPluginControllersWithViews(PluginHost);
    }

    protected override void OnConfigureHost(WebApplication host)
    {
        foreach (var httpModule in host.Services.GetServices<IHttpModule>().PossiblyOrdered())
        {
            httpModule.Register(host.Services, host, host);
        }
    }

    protected override WebApplication OnBuildHost(WebApplicationBuilder builder)
    {
        return builder.Build();
    }
}
