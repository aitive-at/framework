using System.Buffers;

namespace Aitive.Framework.Memory;

internal sealed class SlicedMemoryOwner<T> : IMemoryOwner<T>
{
    private readonly IMemoryOwner<T> _owner;
    private readonly Memory<T> _sliced;

    internal SlicedMemoryOwner(IMemoryOwner<T> owner, Range range)
    {
        _owner = owner;
        _sliced = owner.Memory[range];
    }

    public Memory<T> Memory => _sliced;

    public void Dispose()
    {
        _owner.Dispose();
    }
}

public static class SlicedMemoryOwnerExtensions
{
    extension<T>(IMemoryOwner<T> owner)
    {
        public IMemoryOwner<T> Slice(Range range)
        {
            return new SlicedMemoryOwner<T>(owner, range);
        }

        public IMemoryOwner<T> Slice(int start, int length)
        {
            return new SlicedMemoryOwner<T>(owner, new Range(start, start + length));
        }
    }
}
