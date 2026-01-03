using System.Text.Json.Nodes;

namespace Aitive.Framework.Json;

[Flags]
public enum JsonMergeMode
{
    None = 0x00,
    IfPresent = 0x01,
    IfNotPresent = 0x02,

    ArrayAppend = 0x04,

    ArrayMerge = 0x08,

    Recursive = 0x10,

    PreferTarget = IfNotPresent,
    PreferSource = IfNotPresent | IfPresent,

    Default = PreferSource | Recursive,
}

public static class JsonObjectExtensions
{
    extension(JsonObject jsonObject)
    {
        public JsonObject Merge(JsonObject source, JsonMergeMode mergeMode = JsonMergeMode.Default)
        {
            return source;
        }
    }
}
