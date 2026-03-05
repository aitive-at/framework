namespace Aitive.Framework.Patterns.Disposal;

public static class DisposableExtensions
{
    extension<T>(T item)
    {
        public async ValueTask EnsureDisposal()
        {
            if (item is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
