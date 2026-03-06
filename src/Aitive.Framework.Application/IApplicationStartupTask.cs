namespace Aitive.Framework.Application;

public interface IApplicationStartupTask
{
    ValueTask Execute(CancellationToken cancellationToken = default);
}
