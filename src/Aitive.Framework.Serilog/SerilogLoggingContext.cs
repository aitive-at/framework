using Aitive.Framework.Application.Logging;
using Aitive.Framework.Patterns.Disposal;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Aitive.Framework.Serilog;

public sealed class SerilogLoggingContext : Disposable, ILoggingContext
{
    private readonly SerilogLoggerFactory _factory;

    public SerilogLoggingContext(SerilogLoggerFactory factory)
    {
        Logger = factory.CreateLogger("Boot");
        _factory = factory;
    }

    public ILogger Logger { get; }

    protected override void OnDispose(bool disposing)
    {
        Log.CloseAndFlush();
        _factory.Dispose();
    }
}
