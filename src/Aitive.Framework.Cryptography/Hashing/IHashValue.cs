namespace Aitive.Framework.Cryptography.Hashing;

public interface IHashValue<T>
    where T : struct, IHashValue<T>
{
    static abstract int Size { get; }

    static abstract T Invalid { get; }

    static abstract T FromBytes(ReadOnlySpan<byte> bytes);

    bool IsValid { get; }
    bool IsInvalid { get; }

    ReadOnlyMemory<byte> Data { get; }

    byte[] ToArray();
    string ToHexString();
    string ToBase64String();
    string ToStringPretty();
}
