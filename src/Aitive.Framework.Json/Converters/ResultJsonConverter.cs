using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Json.Converters;

/// <summary>
/// Remarks: For our APIs this is not used right now, maybe at a later date again.
/// </summary>
public sealed class ResultJsonConverterFactory : JsonConverterFactory // TODO: Add [RequiresDynamicCode] in .NET 7
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsResult();

    /// <inheritdoc />
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2026",
        Justification = "No way to annotate the entire class"
    )]
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var underlyingTypes = Result.GetUnderlyingTypes(typeToConvert);

        return Activator.CreateInstance(
                typeof(ResultJsonConverter<,>).MakeGenericType(
                    underlyingTypes.ValueType,
                    underlyingTypes.ErrorType
                )
            ) as JsonConverter;
    }
}

public sealed class ResultJsonConverter<T, TError> : JsonConverter<Result<T, TError>>
{
    public override void Write(
        Utf8JsonWriter writer,
        Result<T, TError> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        if (value.WasSuccessful)
        {
            writer.WriteString("type", "success");
            writer.WritePropertyName("value");

            JsonSerializer.Serialize<T>(writer, value.Value, options);
        }
        else
        {
            writer.WriteString("type", "failure");
            writer.WritePropertyName("value");

            JsonSerializer.Serialize<TError>(writer, value.Error, options);
        }

        writer.WriteEndObject();
    }

    public override Result<T, TError> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? type = null;
        JsonElement? value = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "type":
                        type = reader.ReadRequiredString();
                        break;
                    case "value":
                        value = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                        break;
                }
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                // Finalize
                if (type == null)
                {
                    throw new JsonException($"result type not found");
                }

                if (value == null)
                {
                    throw new JsonException($"value not found");
                }

                var valueReader = value.Value.CreateReader();

                return type switch
                {
                    "success" => JsonSerializer.Deserialize<T>(ref valueReader, options)
                        ?? throw new JsonException(),
                    "failure" => JsonSerializer.Deserialize<TError>(ref valueReader, options)
                        ?? throw new JsonException(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        }

        throw new JsonException("Invalid result");
    }
}
