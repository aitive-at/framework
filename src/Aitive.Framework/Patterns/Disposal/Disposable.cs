namespace Aitive.Framework.Patterns.Disposal;

public abstract class Disposable : IDisposable
{
    private readonly Lock _lockObject = new();
    private readonly bool _throwOnDoubleDispose;
    private volatile bool _isDisposed;

    protected Disposable(bool throwOnDoubleDispose = true)
    {
        _throwOnDoubleDispose = throwOnDoubleDispose;
    }

    public bool IsDisposed => _isDisposed;

    public void Dispose()
    {
        TriggerDispose(true);
    }

    protected void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            RaiseAlreadyDisposed();
        }
    }

    protected virtual void OnBeforeDispose(bool disposing) { }

    protected abstract void OnDispose(bool disposing);

    protected void TriggerDispose(bool disposing)
    {
        if (!_isDisposed)
        {
            lock (_lockObject)
            {
                if (!_isDisposed)
                {
                    OnBeforeDispose(disposing);
                    OnDispose(disposing);
                    _isDisposed = true;
                }
                else
                {
                    if (_throwOnDoubleDispose)
                    {
                        RaiseAlreadyDisposed();
                    }
                }
            }
        }
        else
        {
            if (_throwOnDoubleDispose)
            {
                RaiseAlreadyDisposed();
            }
        }
    }

    private void RaiseAlreadyDisposed()
    {
        throw new ObjectDisposedException(this.ToString() ?? this.GetType().FullName);
    }
}
