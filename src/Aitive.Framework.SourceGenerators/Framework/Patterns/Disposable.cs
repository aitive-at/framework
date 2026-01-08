namespace Aitive.Framework.SourceGenerators.Framework.Patterns;

public abstract class Disposable : IDisposable
{
    private readonly object _lockObject = new();
    private volatile bool _isDisposed;

    public bool IsDisposed => _isDisposed;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            lock (_lockObject)
            {
                if (!_isDisposed)
                {
                    OnDispose();
                    _isDisposed = true;
                }
            }
        }
    }

    protected void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            lock (_lockObject)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(ToString());
                }
            }
        }
    }

    protected abstract void OnDispose();
}
