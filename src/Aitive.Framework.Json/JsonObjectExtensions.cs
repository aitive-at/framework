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
    JsonSimpleMergePolicy TypeMismatch,
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
        JsonSimpleMergePolicy.Write,
        true
    );
}

public static class JsonObjectExtensions
{
    extension(JsonObject jsonObject)
    {
        public (JsonValueType valueType, JsonNode? node) GetPropertyValue(string key)
        {
            if (jsonObject.TryGetPropertyValue(key, out var node))
            {
                return (node.ValueType, node);
            }

            return (JsonValueType.Undefined, null);
        }

        public JsonObject Merge(JsonObject source, JsonMergePolicy? mergePolicy = null)
        {
            var policy = mergePolicy ?? JsonMergePolicy.Default;
            var result = new JsonObject();

            // Start with all target properties (deep cloned)
            foreach (var (key, node) in jsonObject)
            {
                result[key] = node?.DeepClone();
            }

            // Process each source property against what's already in result
            foreach (var (key, _) in source)
            {
                var sourceValue = source.GetPropertyValue(key);
                var targetValue = jsonObject.GetPropertyValue(key);

                bool targetExists = targetValue.valueType != JsonValueType.Undefined;
                bool compatible = sourceValue.valueType.IsCompatibleWith(targetValue.valueType);

                // Type mismatch between existing target and source
                if (targetExists && !compatible)
                {
                    if (policy.TypeMismatch == JsonSimpleMergePolicy.Write)
                    {
                        result[key] = sourceValue.node?.DeepClone();
                    }

                    continue;
                }

                switch (sourceValue.valueType)
                {
                    case JsonValueType.Object:
                    {
                        if (!targetExists)
                        {
                            if (policy.ObjectNotPresent == JsonSimpleMergePolicy.Write)
                            {
                                result[key] = sourceValue.node?.DeepClone();
                            }

                            break;
                        }

                        switch (policy.ObjectPresent)
                        {
                            case JsonObjectMergePolicy.Ignore:
                                break;
                            case JsonObjectMergePolicy.Write:
                                result[key] = sourceValue.node?.DeepClone();
                                break;
                            case JsonObjectMergePolicy.Merge:
                                if (
                                    policy.Recurse
                                    && targetValue.node is JsonObject targetObj
                                    && sourceValue.node is JsonObject sourceObj
                                )
                                {
                                    result[key] = targetObj.Merge(sourceObj, policy);
                                }
                                else
                                {
                                    result[key] = sourceValue.node?.DeepClone();
                                }

                                break;
                        }

                        break;
                    }

                    case JsonValueType.Array:
                    {
                        if (!targetExists)
                        {
                            if (policy.ArrayNotPresent == JsonSimpleMergePolicy.Write)
                            {
                                result[key] = sourceValue.node?.DeepClone();
                            }

                            break;
                        }

                        switch (policy.ArrayPresent)
                        {
                            case JsonArrayMergePolicy.Ignore:
                                break;
                            case JsonArrayMergePolicy.Write:
                                result[key] = sourceValue.node?.DeepClone();
                                break;
                            case JsonArrayMergePolicy.Append:
                            {
                                var merged = new JsonArray();
                                if (targetValue.node is JsonArray targetArr)
                                {
                                    foreach (var item in targetArr)
                                    {
                                        merged.Add(item?.DeepClone());
                                    }
                                }

                                if (sourceValue.node is JsonArray sourceArr)
                                {
                                    foreach (var item in sourceArr)
                                    {
                                        merged.Add(item?.DeepClone());
                                    }
                                }

                                result[key] = merged;
                                break;
                            }
                            case JsonArrayMergePolicy.Merge:
                            {
                                var targetArr = targetValue.node as JsonArray;
                                var sourceArr = sourceValue.node as JsonArray;
                                var targetCount = targetArr?.Count ?? 0;
                                var sourceCount = sourceArr?.Count ?? 0;
                                var merged = new JsonArray();

                                for (int i = 0; i < Math.Max(targetCount, sourceCount); i++)
                                {
                                    if (i >= sourceCount)
                                    {
                                        merged.Add(targetArr![i]?.DeepClone());
                                    }
                                    else if (i >= targetCount)
                                    {
                                        merged.Add(sourceArr![i]?.DeepClone());
                                    }
                                    else if (
                                        policy.Recurse
                                        && targetArr![i] is JsonObject tObj
                                        && sourceArr![i] is JsonObject sObj
                                    )
                                    {
                                        merged.Add(tObj.Merge(sObj, policy));
                                    }
                                    else
                                    {
                                        merged.Add(sourceArr![i]?.DeepClone());
                                    }
                                }

                                result[key] = merged;
                                break;
                            }
                        }

                        break;
                    }

                    // Scalars: Null, Boolean, Number, String
                    default:
                    {
                        var applies = targetExists ? policy.ScalarPresent : policy.ScalarNotPresent;

                        if (applies == JsonSimpleMergePolicy.Write)
                            result[key] = sourceValue.node?.DeepClone();
                        break;
                    }
                }
            }

            return result;
        }
    }
}
