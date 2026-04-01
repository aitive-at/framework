namespace Aitive.Framework.Threading;

public sealed class AtomicLong
{
    private long _value;

    public AtomicLong(long value = 0L)
    {
        _value = value;
    }

    public long Value
    {
        get => Volatile.Read(ref _value);
        set => Set(value);
    }

    public long Increment(long increment = 1)
    {
        return Interlocked.Add(ref _value, increment);
    }

    public long Decrement(long decrement = 1)
    {
        return Interlocked.Add(ref _value, -decrement);
    }

    public long Set(long value)
    {
        return Interlocked.Exchange(ref _value, value);
    }

    /// <summary>
    /// Compare Exchange
    /// </summary>
    /// <param name="expected">The value that is compared with and expected</param>
    /// <param name="newValue">The value to set if the current value equals the expected value</param>
    /// <returns>The original value before exchange</returns>
    public long CompareExchange(long expected, long newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected);
    }
}
