using Aitive.Framework.Application;
using Aitive.Framework.Application.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Aitive.Framework.Serilog;

public class SerilogLoggingProvider : ILoggingProvider
{
    public ILoggingContext CreateBoostrapContext(IApplicationDescription applicationDescription)
    {
        var configuration = new LoggerConfiguration();
        ApplyDefaultConfiguration(applicationDescription, configuration);

        Log.Logger = configuration.CreateLogger();

        var factory = new SerilogLoggerFactory(Log.Logger, dispose: false);

        return new SerilogLoggingContext(factory);
    }

    public void ConfigureLogging(
        IApplicationDescription applicationDescription,
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    )
    {
        services.AddSerilog(
            (
                (provider, loggerConfiguration) =>
                {
                    ApplyDefaultConfiguration(applicationDescription, loggerConfiguration);
                    loggerConfiguration.ReadFrom.Services(provider);
                }
            )
        );
    }

    protected virtual string GetLogFilename(IApplicationDescription applicationDescription)
    {
        return Path.Combine(AppContext.BaseDirectory, "logs", applicationDescription.Id + ".log");
    }

    protected virtual void ApplyDefaultConfiguration(
        IApplicationDescription applicationDescription,
        LoggerConfiguration loggerConfiguration
    )
    {
        loggerConfiguration
            .WriteTo.Console()
            .WriteTo.File(GetLogFilename(applicationDescription))
            .Enrich.FromLogContext();
    }
}
