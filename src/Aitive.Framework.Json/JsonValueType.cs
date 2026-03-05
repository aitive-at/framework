using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aitive.Framework.Json;

public enum JsonValueType
{
    Undefined,
    Null,
    Boolean,
    Number,
    String,
    Object,
    Array,
}

public static class JsonValueExtensions
{
    extension(JsonNode? node)
    {
        public JsonValueType ValueType
        {
            get
            {
                if (node is null)
                {
                    return JsonValueType.Null;
                }

                return node.GetValueKind() switch
                {
                    JsonValueKind.Undefined => JsonValueType.Undefined,
                    JsonValueKind.Object => JsonValueType.Object,
                    JsonValueKind.Array => JsonValueType.Array,
                    JsonValueKind.String => JsonValueType.String,
                    JsonValueKind.Number => JsonValueType.Number,
                    JsonValueKind.True => JsonValueType.Boolean,
                    JsonValueKind.False => JsonValueType.Boolean,
                    JsonValueKind.Null => JsonValueType.Null,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        }
    }

    extension(JsonValueType valueType)
    {
        public bool IsCompatibleWith(JsonValueType otherValueType)
        {
            if (valueType == otherValueType)
            {
                return true;
            }

            if (valueType == JsonValueType.Null || otherValueType == JsonValueType.Null)
            {
                return true;
            }

            if (valueType == JsonValueType.Undefined || otherValueType == JsonValueType.Undefined)
            {
                return true;
            }

            return false;
        }
    }
}
