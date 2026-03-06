using Aitive.Framework.Patterns.Disposal;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Diagnostics.Logging;

public sealed class NullLogger : ILogger
{
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) { }

    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return IdemPotentDisposable.Instance;
    }
}
