using System.Runtime.InteropServices;

namespace Aitive.Framework.Cryptography.Hashing.Algorithms;

public readonly struct Sha256Value
    : IParsable<Sha256Value>,
        IComparable<Sha256Value>,
        IHashValue<Sha256Value>,
        IEquatable<Sha256Value>
{
    public static int Size { get; } = 32;
    public static int HexStringSize { get; } = Size * 2;

    public static Sha256Value Zero { get; } = new(new byte[Size]);
    public static Sha256Value Invalid { get; } = Zero;

    public static Sha256Value FromBytes(ReadOnlySpan<byte> bytes)
    {
        return new Sha256Value(bytes.ToArray());
    }

    public bool IsValid => !Equals(Invalid);
    public bool IsInvalid => Equals(Invalid);

    public ReadOnlyMemory<byte> Data { get; }

    public static implicit operator ReadOnlyMemory<byte>(Sha256Value value) => value.Data;

    public static implicit operator Sha256Value(ReadOnlyMemory<byte> value) => new(value);

    public static implicit operator Sha256Value(byte[] value) => new(value);

    public static implicit operator byte[](Sha256Value value) => value.Data.ToArray();

    public Sha256Value(ReadOnlyMemory<byte> data)
    {
        if (data.Length != Size)
        {
            throw new InvalidOperationException($"SHA256 hash must be {Size} bytes long");
        }

        Data = data;
    }

    public byte[] ToArray() => Data.ToArray();

    public int CompareTo(Sha256Value other)
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

    public static Sha256Value Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var hashValue))
        {
            throw new FormatException();
        }

        return hashValue;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Sha256Value result)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            try
            {
                var actualString = s.Replace(" ", string.Empty);
                var hashBytes = Convert.FromHexString(actualString);
                result = new Sha256Value(hashBytes);
                return true;
            }
            catch (FormatException) { }
        }

        result = new Sha256Value(Array.Empty<byte>());
        return false;
    }

    public bool Equals(Sha256Value other)
    {
        return Data.Span.SequenceEqual(other.Data.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is Sha256Value other && Equals(other);
    }

    public static bool operator ==(Sha256Value left, Sha256Value right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Sha256Value left, Sha256Value right)
    {
        return !left.Equals(right);
    }
}
