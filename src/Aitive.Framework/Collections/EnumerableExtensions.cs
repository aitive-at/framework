using Aitive.Framework.Functional;

namespace Aitive.Framework.Collections;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
        where T : IOrdered
    {
        public IOrderedEnumerable<T> Ordered()
        {
            return enumerable.OrderBy(x => x.Order);
        }
    }

    extension<T>(IEnumerable<T> enumerable)
    {
        public bool None() => !enumerable.Any();

        public bool Empty() => !enumerable.Any();

        public Optional<T> FirstOrOptional(Func<T, bool> predicate)
        {
            foreach (var item in enumerable)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return Optional<T>.None;
        }

        public Optional<T> FirstOrOptional()
        {
            foreach (var item in enumerable)
            {
                return item;
            }

            return Optional<T>.None;
        }

        public Optional<T> LastOrOptional(Func<T, bool> predicate)
        {
            if (enumerable.TryGetNonEnumeratedCount(out var count))
            {
                var index = count - 1;

                while (index >= 0)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var element = enumerable.ElementAt(index);

                    if (predicate(element))
                    {
                        return element;
                    }

                    index--;
                }

                return Optional<T>.None;
            }

            Optional<T> result = Optional<T>.None;
            foreach (var item in enumerable)
            {
                if (predicate(item))
                {
                    result = item;
                }
            }

            return result;
        }

        public Optional<T> LastOrOptional()
        {
            if (enumerable.TryGetNonEnumeratedCount(out var count))
            {
                if (count >= 0)
                {
                    return enumerable.ElementAt(count - 1);
                }
            }

            Optional<T> result = Optional<T>.None;
            foreach (var item in enumerable)
            {
                result = item;
            }

            return result;
        }

        public IEnumerable<T> PossiblyOrdered(int defaultOrder = 0)
        {
            return enumerable
                .Select(o =>
                {
                    if (o is IOrdered ordered)
                    {
                        return (o, ordered.Order);
                    }

                    return (o, defaultOrder);
                })
                .OrderBy(o => o.Item2)
                .Select(o => o.o);
        }
    }
}
