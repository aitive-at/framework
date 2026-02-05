using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aitive.Framework.Json.Converters;

public abstract class StringProxyJsonConverter<T> : JsonConverter<T>
    where T : notnull
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var stringValue = reader.GetString();

        var resolvedValue =
            stringValue
            ?? throw new InvalidOperationException($"{typeof(T)} cannot be null in Json");

        return TryParse(resolvedValue, out var result)
            ? result
            : throw new JsonException($"Cannot parse {typeof(T)} from {resolvedValue}");
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
        writer.WriteStringValue(ToString(value));
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        writer.WritePropertyName(ToString(value));
    }

    protected abstract bool TryParse(string value, [NotNullWhen(true)] out T? parsedValue);

    protected abstract string ToString(T value);
}
