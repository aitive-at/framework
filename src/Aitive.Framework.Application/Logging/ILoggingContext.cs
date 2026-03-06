using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Application.Logging;

public interface ILoggingContext : IDisposable
{
    ILogger Logger { get; }
}
