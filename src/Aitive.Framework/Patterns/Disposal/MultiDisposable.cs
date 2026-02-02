namespace Aitive.Framework.Patterns.Disposal;

public class MultiDisposable : Disposable
{
    private readonly List<IDisposable> _disposables;

    public MultiDisposable(IEnumerable<IDisposable> disposables, bool throwOnDoubleDispose = true)
        : base(throwOnDoubleDispose)
    {
        _disposables = disposables.ToList();
    }

    protected override void OnDispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
