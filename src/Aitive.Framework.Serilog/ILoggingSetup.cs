using Microsoft.Extensions.Configuration;
using Serilog;

namespace Aitive.Framework.Serilog;

public interface ILoggingSetup
{
    LoggerConfiguration ConfigureBootstrap(LoggerConfiguration loggerConfiguration);

    LoggerConfiguration Configure(
        IServiceProvider serviceProvider,
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration
    );
}
