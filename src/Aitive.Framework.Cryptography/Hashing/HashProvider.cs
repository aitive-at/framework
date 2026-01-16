using System.Buffers;
using Microsoft.IO;

namespace Aitive.Framework.Cryptography.Hashing;

public sealed class HashProvider<T>(
    IHashAlgorithm<T> algorithm,
    RecyclableMemoryStreamManager streamManager
) : IHashProvider<T>
    where T : struct, IHashValue<T>
{
    IHashAlgorithm<T> IHashProvider<T>.Algorithm => algorithm;

    public HashBuilder<T> CreateBuilder()
    {
        return new HashBuilder<T>(algorithm, streamManager.GetStream(), ArrayPool<byte>.Shared);
    }
}
