namespace Aitive.Framework.Interop.Libraries;

/// <summary>
/// Represents a loaded native library with symbol lookup capability.
/// This is a general-purpose interface that can be used across multiple native library wrappers.
/// </summary>
public interface INativeLibrary : IDisposable
{
    /// <summary>
    /// Gets the native handle to the loaded library.
    /// </summary>
    nint Handle { get; }

    /// <summary>
    /// Finds a symbol (function or variable) by name in the library.
    /// </summary>
    /// <param name="name">The symbol name to find.</param>
    /// <returns>The address of the symbol, or 0 if not found.</returns>
    nint FindSymbol(string name);

    /// <summary>
    /// Gets a typed function pointer for a symbol.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type for the function.</typeparam>
    /// <param name="name">The symbol name.</param>
    /// <returns>The delegate, or null if not found.</returns>
    TDelegate? GetFunction<TDelegate>(string name)
        where TDelegate : Delegate;
}
