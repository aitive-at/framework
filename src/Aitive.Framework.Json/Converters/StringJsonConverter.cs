using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aitive.Framework.Json.Converters;

public class StringJsonConverter<T> : JsonConverter<T>
    where T : notnull, IParsable<T>
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var stringValue = reader.GetString();

        return T.Parse(
            stringValue
                ?? throw new InvalidOperationException($"{typeof(T)} cannot be null in Json"),
            null
        );
    }

    public override T ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        writer.WritePropertyName(
            value.ToString() ?? throw new JsonException($"{typeof(T)} ToString returned null")
        );
    }
}
