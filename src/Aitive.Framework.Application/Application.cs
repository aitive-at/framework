using System.Runtime.ExceptionServices;
using Aitive.Framework.Collections;
using Aitive.Framework.Configuration.Plugins;
using Aitive.Framework.Diagnostics.Exceptions;
using Aitive.Framework.Diagnostics.Logging;
using Aitive.Framework.Functional.Pipelines;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Hosts;
using Microsoft.Extensions.Configuration;
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

            OnSetupConfiguration(Description, builder.Environment, builder.Configuration);

            Options.LoggingProvider.ConfigureLogging(
                Description,
                builder.Services,
                builder.Environment,
                builder.Configuration
            );

            PluginHost = OnCreatePluginHost(builder);

            OnConfigureBuilder(builder);

            builder.Services.AddSingleton<TSelf>((TSelf)this);

            using var host = OnBuildHost(builder);

            Logger = host.Services.GetRequiredService<ILogger<TSelf>>();
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

            await OnRunHost(host, cancellationToken);
        }
        catch (Exception ex)
        {
            var dispatchInfo = ExceptionDispatchInfo.Capture(ex);
            var exceptionContext = new ExceptionHandlerContext(dispatchInfo, Logger);
            exceptionHandler(exceptionContext);
            exceptionContext.RethrowUnhandled();
        }
    }

    protected virtual void OnSetupConfiguration(
        IApplicationDescription description,
        IHostEnvironment environment,
        IConfigurationManager configuration
    )
    {
        var baseJsonFileName = description.Id + ".json";

        configuration.Sources.Clear();

        // 1. Base json
        configuration.AddJsonFile(baseJsonFileName, optional: false, reloadOnChange: false);

        // 2. Environment-specific json
        //    e.g. mysettings.Development.json
        configuration.AddJsonFile(
            $"{System.IO.Path.GetFileNameWithoutExtension(baseJsonFileName)}.{environment.EnvironmentName}{System.IO.Path.GetExtension(baseJsonFileName)}",
            optional: true,
            reloadOnChange: false
        );

        // 4. Environment variables
        configuration.AddEnvironmentVariables();

        // 5. Command-line args
        configuration.AddCommandLine(Environment.GetCommandLineArgs());
    }

    protected abstract TBuilder OnCreateBuilder();

    protected virtual void OnConfigureBuilder(TBuilder builder)
    {
        builder.Services.BindPluginServices(PluginHost);
        builder.Services.BindPluginConfigurationOptions(PluginHost, builder.Configuration);
    }

    protected abstract void OnConfigureHost(THost host);

    protected abstract THost OnBuildHost(TBuilder builder);

    protected virtual IPluginHost OnCreatePluginHost(TBuilder builder)
    {
        return new NullPluginHost();
    }

    protected virtual Task OnRunHost(THost host, CancellationToken cancellationToken = default)
    {
        return host.RunAsync(cancellationToken);
    }
}
