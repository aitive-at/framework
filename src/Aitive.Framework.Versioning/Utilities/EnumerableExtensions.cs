using System.Runtime.CompilerServices;

namespace Aitive.Framework.Versioning.Utilities;

internal static class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> values) =>
        values.ToList().AsReadOnly();
}
