using System.Text.Json;
using System.Text.Json.Nodes;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Json;

public static class JsonElementExtensions
{
    extension(in JsonElement jsonElement)
    {
        public JsonNode? ToNode()
        {
            return JsonSerializer.Deserialize<JsonNode>(jsonElement.GetRawText());
        }

        public string ReadRequiredString()
        {
            return jsonElement.ValueKind == JsonValueKind.String
                ? jsonElement.GetString()
                    ?? throw new JsonException("Expected json string value, got null")
                : throw new JsonException(
                    $"Expected json string value, got: {jsonElement.ValueKind}"
                );
        }

        public JsonElement ReadRequiredObjectProperty(string propertyName)
        {
            return jsonElement.TryGetProperty(propertyName, out var value)
                ? value.ValueKind == JsonValueKind.Object
                    ? value
                    : throw new JsonException($"Expected json object, got {value.ValueKind}")
                : throw new JsonException($"Property {propertyName} not found");
        }

        public Optional<JsonElement> ReadOptionalObjectProperty(string propertyName)
        {
            return jsonElement.TryGetProperty(propertyName, out var value)
                ? value.ValueKind == JsonValueKind.Object
                    ? value
                    : throw new JsonException($"Expected json object, got {value.ValueKind}")
                : Optional<JsonElement>.None;
        }

        public Utf8JsonReader CreateReader()
        {
            // TODO: Optimize this
            var bytes = JsonSerializer.SerializeToUtf8Bytes(jsonElement);
            return new Utf8JsonReader(bytes);
        }
    }
}
