namespace Aitive.Framework.Patterns.Disposal;

public sealed class AsyncActionDisposable : AsyncDisposable
{
    private readonly Func<ValueTask> _disposalAction;

    public AsyncActionDisposable(Func<ValueTask> disposalAction)
    {
        _disposalAction = disposalAction;
    }

    protected override ValueTask OnDispose()
    {
        return _disposalAction.Invoke();
    }
}
