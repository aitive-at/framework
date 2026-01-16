namespace Aitive.Framework.Cryptography.Hashing;

public interface IHashProvider<T>
    where T : struct, IHashValue<T>
{
    IHashAlgorithm<T> Algorithm { get; }
    HashBuilder<T> CreateBuilder();
}
