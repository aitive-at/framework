using System.Text.Json.Nodes;

namespace Aitive.Framework.Json;

public enum JsonSimpleMergePolicy
{
    Ignore,
    Write,
}

public enum JsonArrayMergePolicy
{
    Ignore,
    Write,
    Append,
    Merge,
}

public enum JsonObjectMergePolicy
{
    Ignore,
    Write,
    Merge,
}

public sealed record JsonMergePolicy(
    JsonSimpleMergePolicy ScalarPresent,
    JsonSimpleMergePolicy ScalarNotPresent,
    JsonArrayMergePolicy ArrayPresent,
    JsonSimpleMergePolicy ArrayNotPresent,
    JsonObjectMergePolicy ObjectPresent,
    JsonSimpleMergePolicy ObjectNotPresent,
    bool Recurse
)
{
    public static readonly JsonMergePolicy Default = new(
        JsonSimpleMergePolicy.Write,
        JsonSimpleMergePolicy.Write,
        JsonArrayMergePolicy.Write,
        JsonSimpleMergePolicy.Write,
        JsonObjectMergePolicy.Write,
        JsonSimpleMergePolicy.Write,
        false
    );
}

public static class JsonObjectExtensions
{
    extension(JsonObject jsonObject)
    {
        public JsonObject Merge(JsonObject source, JsonMergePolicy? mergePolicy = null)
        {
            var actualMergePolicy = mergePolicy ?? JsonMergePolicy.Default;

            var result = new JsonObject();

            foreach (var (key, sourceValue) in source)
            {
                if (jsonObject.TryGetPropertyValue(key, out var targetValue))
                {
                    // Present
                }
                else
                {
                    // Not present
                }
            }

            return source;
        }
    }
}
