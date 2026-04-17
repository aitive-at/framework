using Aitive.Framework.Functional;
using Aitive.Framework.GeneratedCode;

namespace Aitive.Framework.Io;

[TypedId]
public readonly partial record struct Path(string Value)
{
    public bool IsFile => new FileInfo(Value).Exists;
    public bool IsDirectory => new DirectoryInfo(Value).Exists;
    public bool Exists => System.IO.Path.Exists(Value);

    public DirectoryInfo DirectoryInfo => new DirectoryInfo(Value);
    public FileInfo FileInfo => new FileInfo(Value);

    public static Path operator /(Path a, string b)
    {
        return System.IO.Path.Combine(a, b);
    }

    public static Path operator /(Path a, Path b)
    {
        return System.IO.Path.Combine(a, b);
    }

    public static Path operator --(Path a)
    {
        return a.ParentDirectory.Or(a);
    }

    public Path Absolute => System.IO.Path.GetFullPath(Value);

    public long FreeDiskSpace => DriveUtilities.GetAvailableFreeBytes(Value);

    public bool HasExtension => System.IO.Path.HasExtension(Value);

    public Path RelativeTo(Path other)
    {
        return System.IO.Path.GetRelativePath(Value, other.Value);
    }

    /// <summary>
    /// The name of the current path be it a directory or file. Includes extension if it is a file
    /// </summary>
    public Optional<string> Name
    {
        get
        {
            var result = System.IO.Path.GetFileName(Value);
            return !string.IsNullOrEmpty(result) ? Optional.Some(result) : Optional<string>.None;
        }
    }

    /// <summary>
    /// The name of the current path be it a directory file. Removes any extension
    /// </summary>
    public Optional<string> NameWithoutExtension
    {
        get
        {
            var result = System.IO.Path.GetFileNameWithoutExtension(Value);
            return !string.IsNullOrEmpty(result) ? Optional.Some(result) : Optional<string>.None;
        }
    }

    public Optional<string> Extension
    {
        get
        {
            var result = System.IO.Path.GetExtension(Value);
            return !string.IsNullOrEmpty(result) ? Optional.Some(result) : Optional<string>.None;
        }
    }

    public Path WithoutExtension => System.IO.Path.ChangeExtension(Value, null);

    public Optional<Path> ParentDirectory =>
        System.IO.Path.GetDirectoryName(Value).NullableAsOptional().Select(o => new Path(o));

    public Path WithExtension(string extension)
    {
        return System.IO.Path.ChangeExtension(Value, extension);
    }

    public IEnumerable<Path> EnumerateFiles(
        string pattern = "*.*",
        bool recursive = false,
        bool ignoreInaccessible = true,
        bool returnSpecial = true
    )
    {
        return Directory
            .EnumerateFiles(
                Value,
                pattern,
                new EnumerationOptions()
                {
                    RecurseSubdirectories = recursive,
                    IgnoreInaccessible = ignoreInaccessible,
                    ReturnSpecialDirectories = returnSpecial,
                }
            )
            .Select(o => new Path(o));
    }

    public IEnumerable<Path> EnumerateChildDirectories(
        bool recursive = false,
        bool ignoreInaccessible = true,
        bool returnSpecial = true
    )
    {
        return Directory
            .EnumerateDirectories(
                Value,
                "*",
                new EnumerationOptions()
                {
                    RecurseSubdirectories = recursive,
                    IgnoreInaccessible = ignoreInaccessible,
                    ReturnSpecialDirectories = returnSpecial,
                }
            )
            .Select(o => new Path(o));
    }
}
