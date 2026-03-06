namespace Aitive.Framework.Patterns.Disposal;

public class MultiAsyncDisposable : IAsyncDisposable
{
    private readonly IReadOnlyList<IAsyncDisposable> _disposables;

    public MultiAsyncDisposable(IEnumerable<IAsyncDisposable> disposables)
    {
        _disposables = disposables.ToList();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in _disposables)
        {
            await disposable.DisposeAsync();
        }
    }
}
