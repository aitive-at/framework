using System.Security.Cryptography;
using Microsoft.Extensions.ObjectPool;

namespace Aitive.Framework.Cryptography.Hashing.Algorithms;

public sealed class Sha1ClrHashAlgorithm : IHashAlgorithm<Sha1Value>
{
    internal sealed class Sha1HashPoolPolicy : PooledObjectPolicy<SHA1>
    {
        public override SHA1 Create()
        {
            return SHA1.Create();
        }

        public override bool Return(SHA1 obj)
        {
            return true;
        }
    }

    private readonly ObjectPool<SHA1> _pool;

    public Sha1ClrHashAlgorithm()
    {
        _pool = new DefaultObjectPoolProvider().Create<SHA1>(new Sha1HashPoolPolicy());
    }

    public Task ComputeAsync(ReadOnlyMemory<byte> source, Memory<byte> target)
    {
        Compute(source.Span, target.Span);
        return Task.CompletedTask;
    }

    public async Task<Sha1Value> ComputeAsync(
        Stream source,
        CancellationToken cancellationToken = default
    )
    {
        var algorithm = _pool.Get();

        try
        {
            var hash = await algorithm.ComputeHashAsync(source, cancellationToken);
            return new Sha1Value(hash);
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

    public Sha1Value Compute(Stream source)
    {
        var algorithm = _pool.Get();

        try
        {
            var hash = algorithm.ComputeHash(source);
            return new Sha1Value(hash);
        }
        finally
        {
            _pool.Return(algorithm);
        }
    }
}
