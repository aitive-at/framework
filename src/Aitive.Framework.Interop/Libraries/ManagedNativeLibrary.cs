using System.Runtime.InteropServices;
using Aitive.Framework.Patterns.Disposal;

namespace Aitive.Framework.Interop.Libraries;

/// <summary>
/// Default implementation of <see cref="INativeLibrary"/> using .NET's NativeLibrary.
/// </summary>
public sealed class ManagedNativeLibrary : Finalizable, INativeLibrary
{
    private nint _handle;

    /// <summary>
    /// Creates a new instance wrapping an already-loaded library handle.
    /// </summary>
    /// <param name="handle">The native library handle.</param>
    public ManagedNativeLibrary(nint handle)
    {
        _handle = handle;
    }

    /// <inheritdoc/>
    public nint Handle => _handle;

    /// <inheritdoc/>
    public nint FindSymbol(string name)
    {
        ThrowIfDisposed();

        if (_handle == 0)
        {
            return 0;
        }

        return NativeLibrary.TryGetExport(_handle, name, out var address) ? address : 0;
    }

    /// <inheritdoc/>
    public TDelegate? GetFunction<TDelegate>(string name)
        where TDelegate : Delegate
    {
        var address = FindSymbol(name);

        return address == 0 ? null : Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }

    protected override void OnDispose(bool disposing)
    {
        NativeLibrary.Free(_handle);
        _handle = 0;
    }
}
