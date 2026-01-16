using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aitive.Framework.Cryptography.Hashing;

public readonly struct HashBuilder<T> : IDisposable
    where T : struct, IHashValue<T>
{
    private readonly IHashAlgorithm<T> _hashAlgorithm;
    private readonly MemoryStream _bufferStream;
    private readonly ArrayPool<byte> _arrayPool;

    internal HashBuilder(
        IHashAlgorithm<T> hashAlgorithm,
        MemoryStream bufferStream,
        ArrayPool<byte> arrayPool
    )
    {
        this._hashAlgorithm = hashAlgorithm;
        this._bufferStream = bufferStream;
        this._arrayPool = arrayPool;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(bool value)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = value ? (byte)1 : (byte)0;
        _bufferStream.Write(buffer);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(char value)
    {
        EnsureOpen();

        Span<char> input = stackalloc char[1];
        input[0] = value;

        Span<byte> buffer = stackalloc byte[3];
        var byteCount = Encoding.UTF8.GetBytes(input, buffer);

        _bufferStream.Write(buffer[..byteCount]);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(byte value)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = value;

        _bufferStream.Write(buffer);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(sbyte value)
    {
        return Write((byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(short value)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        _bufferStream.Write(buffer);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(int value)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        _bufferStream.Write(buffer);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(long value)
    {
        EnsureOpen();
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        _bufferStream.Write(buffer);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(float value)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        _bufferStream.Write(buffer);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(double value)
    {
        EnsureOpen();
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        _bufferStream.Write(buffer);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(string value)
    {
        Write((ReadOnlySpan<char>)value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(TimeSpan value)
    {
        return Write(value.Ticks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(DateTimeOffset value)
    {
        return Write(value.Ticks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(ReadOnlySpan<char> value)
    {
        EnsureOpen();

        var encoding = Encoding.UTF8;

        if (value.Length < 42) // This is taken from CoreCLR implementation
        {
            Span<byte> buffer = stackalloc byte[value.Length * 3];
            var actualByteCount = encoding.GetBytes(value, buffer);

            _bufferStream.Write(buffer[..actualByteCount]);
        }
        else
        {
            var buffer = _arrayPool.Rent(value.Length * 3);

            try
            {
                var actualByteCount = encoding.GetBytes(value, buffer);
                var byteSpan = buffer.AsSpan(0, actualByteCount);

                _bufferStream.Write(byteSpan);
            }
            finally
            {
                _arrayPool.Return(buffer);
            }
        }

        return this;
    }

    public HashBuilder<T> Write(Guid guid)
    {
        EnsureOpen();

        Span<byte> buffer = stackalloc byte[16];
        if (!guid.TryWriteBytes(buffer))
        {
            throw new InvalidOperationException("Could not write guid to buffer");
        }

        Write(buffer);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(ReadOnlySpan<byte> value)
    {
        EnsureOpen();
        _bufferStream.Write(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write<THashValue>(THashValue value)
        where THashValue : struct, IHashValue<THashValue>
    {
        EnsureOpen();
        _bufferStream.Write(value.Data.Span);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashBuilder<T> Write(T value)
    {
        EnsureOpen();
        _bufferStream.Write(value.Data.Span);
        return this;
    }

    /// <summary>
    /// Jit time dynamic dispatch.
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <typeparam name="T">The type of the value to write</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <returns>The hash builder</returns>
    public HashBuilder<T> WriteValue<TValue>(TValue value)
    {
        return value switch
        {
            byte b => Write(b),
            sbyte s => Write(s),
            short s => Write(s),
            int i => Write(i),
            long l => Write(l),
            string s => Write(s),
            char c => Write(c),
            float f => Write(f),
            double d => Write(d),
            bool b => Write(b),
            Guid g => Write(g),
            T h => Write(h.Data.Span),
            DateTimeOffset d => Write(d),
            TimeSpan t => Write(t),
            _ => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Cannot dispatch write of type {typeof(T)}"
            ),
        };
    }

    public async Task<T> BuildAsync()
    {
        return await _hashAlgorithm.ComputeAsync(_bufferStream.ToArray());
    }

    public T Build()
    {
        return _hashAlgorithm.Compute(_bufferStream.ToArray());
    }

    public void Dispose()
    {
        _bufferStream.DisposeAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureOpen() { }
}
