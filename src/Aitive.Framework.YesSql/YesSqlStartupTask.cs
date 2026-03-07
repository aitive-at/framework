using Aitive.Framework.Application;
using YesSql;

namespace Aitive.Framework.YesSql;

public sealed class YesSqlStartupTask : IApplicationStartupTask
{
    private readonly IStore _store;

    public YesSqlStartupTask(IStore store)
    {
        _store = store;
    }

    public async ValueTask Execute(CancellationToken cancellationToken = default)
    {
        await _store.InitializeAsync(cancellationToken);
    }
}
