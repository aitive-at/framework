using Aitive.Framework.Application.Logging;
using Aitive.Framework.Diagnostics.Exceptions;
using Aitive.Framework.Plugins;

namespace Aitive.Framework.Application;

public class ApplicationOptions
{
    public ILoggingProvider LoggingProvider { get; init; } = new NullLoggingProvider();

    public IReadOnlyList<IExceptionHandler> ExceptionHandlers { get; init; } =
        Array.Empty<IExceptionHandler>();
}
