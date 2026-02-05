namespace Aitive.Framework.Collections;

public static class CollectionExtensions
{
    extension<T, TCollection>(TCollection collection)
        where TCollection : ICollection<T>
    {
        public TCollection AddAll(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }

            return collection;
        }
    }
}
