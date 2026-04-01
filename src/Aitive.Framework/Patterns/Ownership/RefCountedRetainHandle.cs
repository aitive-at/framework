using Aitive.Framework.Threading;

namespace Aitive.Framework.Patterns.Ownership;

public abstract class RefCountedRetainHandle<TKey, TValue> : IRetainHandle<TKey, TValue>
    where TKey : notnull
{
    private readonly AtomicLong _refCount;

    protected RefCountedRetainHandle(TKey key, TValue value, AtomicLong refCount)
    {
        Key = key;
        Value = value;
        _refCount = refCount;
    }

    public long RefCount => _refCount.Value;
    public TKey Key { get; }
    public TValue Value { get; }

    public async ValueTask DisposeAsync() { }
}

public abstract class RefCountedCollection<THandle, TKey, TValue>
    where TKey : notnull
    where THandle : IRetainHandle<TKey, TValue>
{
    protected abstract THandle CreateHandle(TKey key, TValue value);
}
