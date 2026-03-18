using Aitive.Framework.Diagnostics.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Application.Logging;

public sealed class NullLoggingContext : ILoggingContext
{
    public void Dispose() { }

    public ILogger Logger { get; } = new NullLogger();
}

public sealed class NullLoggingProvider : ILoggingProvider
{
    public ILoggingContext CreateBoostrapContext(
        IHostEnvironment environment,
        IApplicationDescription applicationDescription
    )
    {
        return new NullLoggingContext();
    }

    public void ConfigureLogging(
        IApplicationDescription applicationDescription,
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    ) { }
}
