namespace Aitive.Framework.Patterns.Disposal;

public abstract class AsyncDisposable : IAsyncDisposable
{
    private readonly object _lockObject = new();

    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        var executeDisposal = false;

        if (!IsDisposed)
        {
            lock (_lockObject)
            {
                if (!IsDisposed)
                {
                    executeDisposal = true;
                    IsDisposed = true;
                }
            }
        }

        if (executeDisposal)
        {
            await OnDispose();
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

    protected abstract ValueTask OnDispose();
}
