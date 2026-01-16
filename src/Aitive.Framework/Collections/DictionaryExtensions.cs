namespace Aitive.Framework.Collections;

public static class DictionaryExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            dictionary.Add(key, value);
            return value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            var value = valueFactory(key);
            dictionary.Add(key, value);
            return value;
        }
    }
}
