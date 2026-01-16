namespace Aitive.Framework.Cryptography.Hashing;

/// <summary>
/// Abstraction for basic hashing.
/// This is needed because the .NET implementation does not ( and probably can't ) delegate
/// to the in browser implementation for dotnet wasm projects. This allows to interop with the
/// actual (async running in a background browser thread ) browser implementation for such projects.
/// </summary>
public interface IHashAlgorithm<T>
    where T : struct, IHashValue<T>
{
    Task ComputeAsync(ReadOnlyMemory<byte> source, Memory<byte> target);
    Task<T> ComputeAsync(Stream source, CancellationToken cancellationToken = default);

    void Compute(ReadOnlySpan<byte> source, Span<byte> target);
    T Compute(Stream source);

    T Compute(ReadOnlySpan<byte> source)
    {
        var result = new byte[T.Size];
        Compute(source, result);
        return T.FromBytes(result);
    }

    async Task<T> ComputeAsync(ReadOnlyMemory<byte> source)
    {
        var result = new byte[T.Size];
        await ComputeAsync(source, result);
        return T.FromBytes(result);
    }
}
