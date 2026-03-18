using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.Application.Logging;

public interface ILoggingProvider
{
    ILoggingContext CreateBoostrapContext(
        IHostEnvironment environment,
        IApplicationDescription applicationDescription
    );

    void ConfigureLogging(
        IApplicationDescription applicationDescription,
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    );
}
