namespace Aitive.Framework.Collections;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public bool None() => !enumerable.Any();

        public bool Empty() => !enumerable.Any();
    }
}
