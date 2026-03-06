using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Diagnostics.Exceptions;

public sealed class ExceptionHandlerContext
{
    private readonly ExceptionDispatchInfo _dispatchInfo;

    public ExceptionHandlerContext(ExceptionDispatchInfo dispatchInfo, ILogger logger)
    {
        _dispatchInfo = dispatchInfo;
        Logger = logger;
        Exception = dispatchInfo.SourceException;
    }

    public bool WasHandled { get; set; }
    public Exception Exception { get; set; }

    public ILogger Logger { get; }

    public void RethrowUnhandled()
    {
        if (!WasHandled)
        {
            if (_dispatchInfo.SourceException == Exception)
            {
                _dispatchInfo.Throw();
            }
            else
            {
                throw Exception;
            }
        }
    }
}
