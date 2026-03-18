using Aitive.Framework.AspNetCore.Plugins;
using Aitive.Framework.Diagnostics.Logging;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Hosts;
using Aitive.Framework.Plugins.Providers;
using Aitive.Framework.Samples.PluginWeb.Plugin01;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

var projectPluginProvider = new ProjectPluginProvider([typeof(TestService).Assembly]);

var pluginHost = new DefaultPluginHostBuilder(new NullLogger())
    .WithProvider(projectPluginProvider)
    .Build(["p1"]);

await using var _ = builder.BindPlugins(pluginHost);
await using var __ = builder.Services.AddMvc().BindPluginControllersWithViews(pluginHost);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.MapRazorComponents<string>().BindPluginRazorComponents(pluginHost);
app.Run();
