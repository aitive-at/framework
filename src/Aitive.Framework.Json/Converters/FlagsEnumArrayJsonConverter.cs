using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aitive.Framework.Json.Converters;

[AttributeUsage(AttributeTargets.Enum)]
public sealed class FlagsEnumJsonOptions : System.Attribute
{
    public FlagsEnumJsonOptions(bool lowerCase = false)
    {
        LowerCase = lowerCase;
    }

    public bool LowerCase { get; }
}

public sealed class FlagsEnumArrayJsonConverterFactory : JsonConverterFactory // TODO: Add [RequiresDynamicCode] in .NET 7
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

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
        return Activator.CreateInstance(
                typeof(FlagsEnumArrayConverter<>).MakeGenericType(typeToConvert)
            ) as JsonConverter;
    }
}

public class FlagsEnumArrayConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly bool _lowerCase;
    private readonly T _defaultValue;
    private readonly bool _ignoreCaseOnReading;

    public FlagsEnumArrayConverter()
        : this(default(T), true) { }

    public FlagsEnumArrayConverter(T defaultValue, bool ignoreCaseOnReading)
    {
        _defaultValue = defaultValue;
        _ignoreCaseOnReading = ignoreCaseOnReading;

        var attribute = typeof(T).GetCustomAttribute<FlagsEnumJsonOptions>();

        if (attribute != null)
        {
            _lowerCase = attribute.LowerCase;
        }
    }

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var flagValue = reader.GetString();

            if (flagValue != null)
            {
                return (T)Enum.Parse(typeof(T), flagValue, ignoreCase: _ignoreCaseOnReading);
            }
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var flags = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                var value = reader.GetString();

                if (value != null)
                {
                    flags.Add(value);
                }
            }

            return (T)
                Enum.Parse(typeof(T), string.Join(", ", flags), ignoreCase: _ignoreCaseOnReading);
        }

        throw new JsonException("Enum flag values must be stored as string or array of strings");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var flagValues = value.ToString().Split([", "], StringSplitOptions.RemoveEmptyEntries);

        if (flagValues.Length == 0)
        {
            writer.WriteStringValue(GetValueToWrite(_defaultValue));
        }
        else if (flagValues.Length == 1)
        {
            writer.WriteStringValue(GetValueToWrite(flagValues[0]));
        }
        else
        {
            writer.WriteStartArray();
            foreach (var flag in flagValues)
            {
                writer.WriteStringValue(GetValueToWrite(flag.Trim()));
            }

            writer.WriteEndArray();
        }
    }

    private string GetValueToWrite(T value)
    {
        var result = value.ToString();

        return GetValueToWrite(result);
    }

    private string GetValueToWrite(string value)
    {
        return _lowerCase ? value.ToLowerInvariant() : value;
    }
}
