using System.Diagnostics.CodeAnalysis;

namespace Aitive.Framework.Interop.Libraries;

/// <summary>
/// Interface for resolving and loading native libraries.
/// Implement this interface to customize how native libraries are located and loaded.
/// </summary>
public interface INativeLibraryResolver
{
    /// <summary>
    /// Attempts to load a native library by name or path.
    /// </summary>
    /// <param name="libraryNameOrPath">The library name (e.g., "dxcompiler") or full path.</param>
    /// <param name="library">The loaded library if successful.</param>
    /// <returns>True if the library was loaded successfully; otherwise, false.</returns>
    bool TryLoadLibrary(string libraryNameOrPath, [NotNullWhen(true)] out INativeLibrary? library);

    /// <summary>
    /// Loads a native library by name or path.
    /// </summary>
    /// <param name="libraryNameOrPath">The library name or full path.</param>
    /// <returns>The loaded library.</returns>
    /// <exception cref="DllNotFoundException">Thrown if the library cannot be found.</exception>
    INativeLibrary LoadLibrary(string libraryNameOrPath)
    {
        if (TryLoadLibrary(libraryNameOrPath, out var library))
        {
            return library;
        }

        throw new DllNotFoundException(libraryNameOrPath);
    }
}
