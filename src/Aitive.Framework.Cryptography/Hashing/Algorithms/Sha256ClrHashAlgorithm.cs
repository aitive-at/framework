using System.Security.Cryptography;
using Microsoft.Extensions.ObjectPool;

namespace Aitive.Framework.Cryptography.Hashing.Algorithms;

public sealed class Sha256ClrHashAlgorithm : IHashAlgorithm<Sha256Value>
{
    internal sealed class Sha256HashPoolPolicy : PooledObjectPolicy<SHA256>
    {
        public override SHA256 Create()
        {
            return SHA256.Create();
        }

        public override bool Return(SHA256 obj)
        {
            return true;
        }
    }

    private readonly ObjectPool<SHA256> _pool;

    public Sha256ClrHashAlgorithm()
    {
        _pool = new DefaultObjectPoolProvider().Create<SHA256>(new Sha256HashPoolPolicy());
    }

    public Task ComputeAsync(ReadOnlyMemory<byte> source, Memory<byte> target)
    {
        Compute(source.Span, target.Span);
        return Task.CompletedTask;
    }

    public async Task<Sha256Value> ComputeAsync(
        Stream source,
        CancellationToken cancellationToken = default
    )
    {
        var algorithm = _pool.Get();

        try
        {
            var hash = await algorithm.ComputeHashAsync(source, cancellationToken);
            return new Sha256Value(hash);
        }
        finally
        {
            _pool.Return(algorithm);
        }
    }

    public void Compute(ReadOnlySpan<byte> source, Span<byte> target)
    {
        var algorithm = _pool.Get();
        try
        {
            if (!algorithm.TryComputeHash(source, target, out _))
            {
                throw new InvalidOperationException("Failed to compute hash.");
            }
        }
        finally
        {
            _pool.Return(algorithm);
        }
    }

    public Sha256Value Compute(Stream source)
    {
        var algorithm = _pool.Get();

        try
        {
            var hash = algorithm.ComputeHash(source);
            return new Sha256Value(hash);
        }
        finally
        {
            _pool.Return(algorithm);
        }
    }
}
