using System.Runtime.InteropServices;

namespace Aitive.Framework.Cryptography.Hashing.Algorithms;

public readonly struct Sha1Value
    : IParsable<Sha1Value>,
        IComparable<Sha1Value>,
        IHashValue<Sha1Value>,
        IEquatable<Sha1Value>
{
    public static int Size { get; } = 20;
    public static int HexStringSize { get; } = Size * 2;

    public static Sha1Value Zero { get; } = new(new byte[Size]);

    public static Sha1Value Invalid { get; } = Zero;

    public static Sha1Value FromBytes(ReadOnlySpan<byte> bytes)
    {
        return new Sha1Value(bytes.ToArray());
    }

    public bool IsValid => !Equals(Invalid);
    public bool IsInvalid => Equals(Invalid);

    public static implicit operator ReadOnlyMemory<byte>(Sha1Value value) => value.Data;

    public static implicit operator Sha1Value(ReadOnlyMemory<byte> value) => new(value);

    public static implicit operator Sha1Value(byte[] value) => new(value);

    public static implicit operator byte[](Sha1Value value) => value.Data.ToArray();

    public ReadOnlyMemory<byte> Data { get; }

    public Sha1Value(ReadOnlyMemory<byte> data)
    {
        if (data.Length != Size)
        {
            throw new InvalidOperationException($"SHA hash must be {Size} bytes long");
        }

        Data = data;
    }

    public byte[] ToArray() => Data.ToArray();

    public int CompareTo(Sha1Value other)
    {
        var a = Data.Span;
        var b = other.Data.Span;

        for (var i = 0; i < 32; ++i)
        {
            var difference = a[i] - b[i];

            if (difference != 0)
            {
                return difference;
            }
        }

        return 0;
    }

    public override int GetHashCode()
    {
        return MemoryMarshal.Cast<byte, int>(Data.Span)[0];
    }

    public string ToHexString() => Convert.ToHexString(Data.Span).ToLowerInvariant();

    public string ToBase64String() => Convert.ToBase64String(Data.Span).ToLowerInvariant();

    public string ToStringPretty() =>
        string.Join(" ", ToHexString().Chunk(2).Select(c => new string(c)));

    public override string ToString()
    {
        return ToHexString();
    }

    public static Sha1Value Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var hashValue))
        {
            throw new FormatException();
        }

        return hashValue;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Sha1Value result)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            try
            {
                var actualString = s.Replace(" ", string.Empty);
                var hashBytes = Convert.FromHexString(actualString);
                result = new Sha1Value(hashBytes);
                return true;
            }
            catch (FormatException) { }
        }

        result = new Sha1Value(Array.Empty<byte>());
        return false;
    }

    public bool Equals(Sha1Value other)
    {
        return Data.Span.SequenceEqual(other.Data.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is Sha1Value other && Equals(other);
    }

    public static bool operator ==(Sha1Value left, Sha1Value right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Sha1Value left, Sha1Value right)
    {
        return !left.Equals(right);
    }
}
