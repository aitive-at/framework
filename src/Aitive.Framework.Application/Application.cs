using System.Runtime.ExceptionServices;
using Aitive.Framework.Collections;
using Aitive.Framework.Configuration.Plugins;
using Aitive.Framework.Diagnostics.Exceptions;
using Aitive.Framework.Diagnostics.Logging;
using Aitive.Framework.Functional.Pipelines;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Application;

public abstract class Application<TBuilder, THost, TSelf>
    where TBuilder : IHostApplicationBuilder
    where THost : IHost
    where TSelf : Application<TBuilder, THost, TSelf>
{
    protected Application(ApplicationOptions options, IApplicationDescription description)
    {
        Options = options;
        Description = description;
        Logger = new NullLogger();
        PluginHost = new NullPluginHost();
    }

    public ApplicationOptions Options { get; }

    public IApplicationDescription Description { get; }

    public IPluginHost PluginHost { get; private set; }

    public ILogger Logger { get; private set; }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        var exceptionHandler = Options.ExceptionHandlers.Compile();
        using var loggingContext = Options.LoggingProvider.CreateBoostrapContext(Description);

        Logger = loggingContext.Logger;

        try
        {
            var builder = OnCreateBuilder();

            Options.LoggingProvider.ConfigureLogging(
                Description,
                builder.Services,
                builder.Configuration,
                builder.Environment
            );

            PluginHost = OnCreatePluginHost(builder);

            OnConfigureBuilder(builder);

            builder.Services.AddSingleton<TSelf>((TSelf)this);
            builder.Services.BindPluginServices(PluginHost);
            builder.Services.BindPluginConfigurationOptions(PluginHost, builder.Configuration);

            using var host = OnBuildHost(builder);
            OnConfigureHost(host);

            // Execute startup tasks
            foreach (
                var startupTask in host
                    .Services.GetServices<IApplicationStartupTask>()
                    .PossiblyOrdered()
            )
            {
                await startupTask.Execute(cancellationToken);
            }

            await host.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var dispatchInfo = ExceptionDispatchInfo.Capture(ex);
            var exceptionContext = new ExceptionHandlerContext(dispatchInfo, Logger);
            exceptionHandler(exceptionContext);
            exceptionContext.RethrowUnhandled();
        }
    }

    protected abstract TBuilder OnCreateBuilder();

    protected abstract void OnConfigureBuilder(TBuilder builder);

    protected abstract void OnConfigureHost(THost host);

    protected abstract THost OnBuildHost(TBuilder builder);

    protected abstract IPluginHost OnCreatePluginHost(TBuilder builder);
}
