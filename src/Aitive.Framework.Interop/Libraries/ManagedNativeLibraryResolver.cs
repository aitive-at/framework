using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aitive.Framework.Interop.Libraries;

/// <summary>
/// Default implementation of <see cref="INativeLibraryResolver"/> using .NET's NativeLibrary.
/// Supports customizable search paths and platform-specific library naming.
/// </summary>
public class ManagedNativeLibraryResolver : INativeLibraryResolver
{
    private readonly List<string> _searchPaths = new();
    private readonly Dictionary<string, string> _libraryAliases = new();

    /// <summary>
    /// Gets or sets whether to search system paths after custom search paths.
    /// Default is true.
    /// </summary>
    public bool SearchSystemPaths { get; set; } = true;

    /// <summary>
    /// Gets the list of custom search paths to check before system paths.
    /// Paths are searched in order.
    /// </summary>
    public IReadOnlyList<string> SearchPaths => _searchPaths;

    /// <summary>
    /// Adds a search path for native libraries.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>This resolver for chaining.</returns>
    public ManagedNativeLibraryResolver AddSearchPath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !_searchPaths.Contains(path))
        {
            _searchPaths.Add(path);
        }

        return this;
    }

    /// <summary>
    /// Adds multiple search paths for native libraries.
    /// </summary>
    /// <param name="paths">The directory paths to search.</param>
    /// <returns>This resolver for chaining.</returns>
    public ManagedNativeLibraryResolver AddSearchPaths(params string[] paths)
    {
        foreach (var path in paths)
        {
            AddSearchPath(path);
        }
        return this;
    }

    /// <summary>
    /// Adds a platform-relative search path (relative to the application base directory).
    /// Automatically appends the runtime identifier subdirectory.
    /// </summary>
    /// <param name="basePath">Base path relative to app directory (e.g., "native").</param>
    /// <returns>This resolver for chaining.</returns>
    public ManagedNativeLibraryResolver AddPlatformSearchPath(string basePath = "native")
    {
        var appBase = AppDomain.CurrentDomain.BaseDirectory;
        var rid = GetRuntimeIdentifier();
        var platformPath = Path.Combine(appBase, basePath, rid);

        if (Directory.Exists(platformPath))
        {
            AddSearchPath(platformPath);
        }

        // Also add the base native path as fallback
        var nativePath = Path.Combine(appBase, basePath);

        if (Directory.Exists(nativePath))
        {
            AddSearchPath(nativePath);
        }

        return this;
    }

    /// <summary>
    /// Adds an alias for a library name, allowing platform-specific naming.
    /// </summary>
    /// <param name="alias">The alias name to use in code (e.g., "slang").</param>
    /// <param name="actualName">The actual library name (e.g., "slang.dll" or "libslang.so").</param>
    /// <returns>This resolver for chaining.</returns>
    public ManagedNativeLibraryResolver AddLibraryAlias(string alias, string actualName)
    {
        _libraryAliases[alias] = actualName;
        return this;
    }

    /// <summary>
    /// Configures platform-specific aliases for a library.
    /// </summary>
    /// <param name="baseName">The base library name (e.g., "slang").</param>
    /// <param name="windowsName">Windows library name (default: {baseName}.dll).</param>
    /// <param name="linuxName">Linux library name (default: lib{baseName}.so).</param>
    /// <param name="macOsName">macOS library name (default: lib{baseName}.dylib).</param>
    /// <returns>This resolver for chaining.</returns>
    public ManagedNativeLibraryResolver AddPlatformLibrary(
        string baseName,
        string? windowsName = null,
        string? linuxName = null,
        string? macOsName = null
    )
    {
        string actualName;

        if (OperatingSystem.IsWindows())
        {
            actualName = windowsName ?? $"{baseName}.dll";
        }
        else if (OperatingSystem.IsLinux())
        {
            actualName = linuxName ?? $"lib{baseName}.so";
        }
        else if (OperatingSystem.IsMacOS())
        {
            actualName = macOsName ?? $"lib{baseName}.dylib";
        }
        else
        {
            actualName = baseName;
        }

        _libraryAliases[baseName] = actualName;
        return this;
    }

    /// <inheritdoc/>
    public bool TryLoadLibrary(
        string libraryNameOrPath,
        [NotNullWhen(true)] out INativeLibrary? library
    )
    {
        library = null;

        // Resolve aliases
        var resolvedName = ResolveLibraryName(libraryNameOrPath);

        // If it's an absolute path, try loading directly
        if (Path.IsPathRooted(resolvedName))
        {
            if (TryLoadFromPath(resolvedName, out library))
            {
                return true;
            }
        }

        // Try custom search paths first
        foreach (var searchPath in _searchPaths)
        {
            var fullPath = Path.Combine(searchPath, resolvedName);
            if (TryLoadFromPath(fullPath, out library))
            {
                return true;
            }

            // Also try with platform-specific extension if not already present
            if (!HasLibraryExtension(resolvedName))
            {
                var withExtension = AddPlatformExtension(resolvedName);
                fullPath = Path.Combine(searchPath, withExtension);

                if (TryLoadFromPath(fullPath, out library))
                {
                    return true;
                }
            }
        }

        // Try system paths if enabled
        if (SearchSystemPaths)
        {
            if (NativeLibrary.TryLoad(resolvedName, out var handle))
            {
                library = new ManagedNativeLibrary(handle);
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public INativeLibrary LoadLibrary(string libraryNameOrPath)
    {
        if (TryLoadLibrary(libraryNameOrPath, out var library))
        {
            return library;
        }

        throw new DllNotFoundException(
            $"Unable to load native library '{libraryNameOrPath}'. "
                + $"Searched paths: {string.Join(", ", _searchPaths.DefaultIfEmpty("[system paths only]"))}"
        );
    }

    /// <summary>
    /// Resolves library aliases to actual names.
    /// </summary>
    protected virtual string ResolveLibraryName(string name)
    {
        return _libraryAliases.GetValueOrDefault(name, name);
    }

    private static bool TryLoadFromPath(
        string fullPath,
        [NotNullWhen(true)] out INativeLibrary? library
    )
    {
        library = null;

        if (!File.Exists(fullPath))
        {
            return false;
        }

        if (NativeLibrary.TryLoad(fullPath, out var handle))
        {
            library = new ManagedNativeLibrary(handle);
            return true;
        }

        return false;
    }

    private static bool HasLibraryExtension(string name)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext is ".dll" or ".so" or ".dylib";
    }

    private static string AddPlatformExtension(string name)
    {
        if (OperatingSystem.IsWindows())
        {
            return name + ".dll";
        }

        if (OperatingSystem.IsLinux())
        {
            return "lib" + name + ".so";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "lib" + name + ".dylib";
        }

        return name;
    }

    private static string GetRuntimeIdentifier()
    {
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "x64",
        };

        if (OperatingSystem.IsWindows())
        {
            return $"win-{arch}";
        }

        if (OperatingSystem.IsLinux())
        {
            return $"linux-{arch}";
        }

        if (OperatingSystem.IsMacOS())
        {
            return $"osx-{arch}";
        }

        return $"unknown-{arch}";
    }
}
