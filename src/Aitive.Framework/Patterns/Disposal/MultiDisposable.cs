namespace Aitive.Framework.Patterns.Disposal;

public sealed class MultiDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;

    public MultiDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables = disposables.ToList();
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
