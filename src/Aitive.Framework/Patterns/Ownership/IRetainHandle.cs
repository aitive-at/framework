namespace Aitive.Framework.Patterns.Ownership;

public interface IRetainHandle<out TKey, out TValue> : IAsyncDisposable
    where TKey : notnull
{
    TKey Key { get; }
    TValue Value { get; }
}
