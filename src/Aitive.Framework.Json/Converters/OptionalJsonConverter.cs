using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Json.Converters;

public sealed class OptionalJsonConverterFactory : JsonConverterFactory // TODO: Add [RequiresDynamicCode] in .NET 7
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsOptional();

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
        var underlyingType = Optional.GetUnderlyingType(typeToConvert);
        return underlyingType is null
            ? null
            : Activator.CreateInstance(
                typeof(OptionalJsonConverter<>).MakeGenericType(underlyingType)
            ) as JsonConverter;
    }
}

public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2026",
        Justification = "Public properties/fields are preserved"
    )]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1013", Justification = "False positive")]
    public override void Write(
        Utf8JsonWriter writer,
        Optional<T> value,
        JsonSerializerOptions options
    )
    {
        switch (value)
        {
            case { IsUndefined: true }:
                writer.WriteNullValue(); // We treat undefined as null when writing
                break;
            case { IsNull: true }:
                writer.WriteNullValue();
                break;
            default:
                // TODO: Attempt to extract IJsonTypeInfo resolver for type T in .NET 7 to avoid Reflection
                JsonSerializer.Serialize(writer, value.ValueOrDefault, options);
                break;
        }
    }

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2026",
        Justification = "Public properties/fields are preserved"
    )]
    public override Optional<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        reader.TokenType is JsonTokenType.Null
            ? Optional.None<T>()
            : JsonSerializer.Deserialize<T>(ref reader, options);
}
