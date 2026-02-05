using System.Text.Json;

namespace Aitive.Framework.Json;

public static class Utf8JsonReaderExtensions
{
    extension(ref Utf8JsonReader reader)
    {
        public string ReadRequiredStringPropertyUntilEnd(
            string propertyName,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase
        )
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 0)
                {
                    var foundPropertyName = reader.GetString();

                    if (
                        foundPropertyName != null
                        && propertyName.Equals(foundPropertyName, stringComparison)
                    )
                    {
                        if (reader.Read() && reader.TokenType == JsonTokenType.String)
                        {
                            return reader.GetString()
                                ?? throw new JsonException($"Expected string, found null");
                        }

                        throw new JsonException($"Expected string, found {reader.TokenType}");
                    }
                }

                // Skip nested objects or arrays completely.
                if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    reader.Skip();
                }
            }

            throw new JsonException($"Expected property {propertyName}, found end of json");
        }

        public string ReadRequiredPropertyName()
        {
            reader.ReadRequiredToken(JsonTokenType.PropertyName);

            return reader.GetString()
                ?? throw new JsonException($"Expected property name, found null");
        }

        public string ReadRequiredString()
        {
            reader.ReadRequiredToken(JsonTokenType.String);

            return reader.GetString() ?? throw new JsonException($"Expected string, found null");
        }

        public int ReadRequiredInt()
        {
            reader.ReadRequiredToken(JsonTokenType.Number);

            if (reader.TryGetInt32(out var value))
            {
                return value;
            }

            throw new JsonException($"Expected int, found {reader.TokenType}");
        }

        public void ReadRequiredToken(JsonTokenType tokenType)
        {
            if (!reader.Read())
            {
                throw new JsonException($"Expected {tokenType}, got empty");
            }

            if (reader.TokenType != tokenType)
            {
                throw new JsonException($"Expected {tokenType}, got {reader.TokenType}");
            }
        }

        public (string, string) ReadRequiredStringProperty()
        {
            var propertyName = reader.ReadRequiredPropertyName();
            var value = reader.ReadRequiredString();

            return (propertyName, value);
        }
    }
}
