using System.Runtime.ExceptionServices;

namespace Aitive.Framework.Diagnostics.Exceptions;

public sealed class ExceptionHandlerContext
{
    private readonly ExceptionDispatchInfo _dispatchInfo;

    internal ExceptionHandlerContext(ExceptionDispatchInfo dispatchInfo)
    {
        _dispatchInfo = dispatchInfo;
        Exception = dispatchInfo.SourceException;
    }

    public bool WasHandled { get; set; }
    public Exception Exception { get; set; }

    internal void RethrowUnhandled()
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
