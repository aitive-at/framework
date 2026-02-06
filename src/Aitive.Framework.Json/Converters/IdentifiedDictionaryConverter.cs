using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aitive.Framework.Json.Converters;

public interface IIdentified<TId>
{
    TId Id { get; set; }
}

public class IdentifiedDictionaryConverter<TDict, TId, TValue> : JsonConverter<TDict>
    where TValue : IIdentified<TId>
    where TId : notnull
{
    public override TDict Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        var dict = new Dictionary<TId, TValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName");
            }

            // All keys are strings in JSON — deserialize to TId
            var keyString =
                reader.GetString() ?? throw new JsonException("Dictionary key cannot be null");

            var key =
                JsonSerializer.Deserialize<TId>($"\"{keyString}\"", options)
                ?? throw new JsonException($"Cannot parse key '{keyString}' as {typeof(TId)}");

            reader.Read();

            var value =
                JsonSerializer.Deserialize<TValue>(ref reader, options)
                ?? throw new JsonException($"Failed to deserialize value of type {typeof(TValue)}");

            value.Id = key;
            dict[key] = value;
        }

        if (typeof(TDict).IsAssignableFrom(typeof(Dictionary<TId, TValue>)))
        {
            return (TDict)(object)dict;
        }

        throw new JsonException($"Cannot produce instance of {typeof(TDict)}");
    }

    public override void Write(Utf8JsonWriter writer, TDict value, JsonSerializerOptions options)
    {
        var enumerable =
            value as IEnumerable<KeyValuePair<TId, TValue>>
            ?? throw new JsonException($"Cannot enumerate {typeof(TDict)}");

        var idPropertyName = options.PropertyNamingPolicy?.ConvertName("Id") ?? "Id";

        writer.WriteStartObject();

        foreach (var kvp in enumerable)
        {
            // Key: serialize TId to its string representation
            var keyStr =
                JsonSerializer.Serialize(kvp.Key, options).Trim('"')
                ?? throw new JsonException("Dictionary key cannot be null");

            writer.WritePropertyName(keyStr);

            // Value: serialize using runtime type, then strip the Id property
            using var doc = JsonSerializer.SerializeToDocument(
                kvp.Value,
                kvp.Value!.GetType(),
                options
            );
            writer.WriteStartObject();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name.Equals(idPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                prop.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}
