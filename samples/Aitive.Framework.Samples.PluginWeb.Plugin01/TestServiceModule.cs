using System.Reflection;
using Aitive.Framework.DependencyInjection;
using Aitive.Framework.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Samples.PluginWeb.Plugin01;

public class TestServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<TestService>();
    }
}
