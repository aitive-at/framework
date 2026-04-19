using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Functional;
using Aitive.Framework.Reflection;

namespace Aitive.Framework.Json.Converters;

public class PolymorphicResolverDictionaryJsonConverter<TType, T>(ITypeResolver<TType> typeResolver)
    : PolymorphicDictionaryJsonConverter<TType, T>
    where TType : notnull
{
    protected override Optional<Type> GetValueType(
        TType type,
        JsonElement data,
        JsonSerializerOptions options
    )
    {
        return typeResolver.Resolve(type);
    }
}

public abstract class PolymorphicDictionaryJsonConverter<TType, T>
    : JsonConverter<IReadOnlyDictionary<TType, T?>>
    where TType : notnull
{
    public override IReadOnlyDictionary<TType, T?> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        Dictionary<TType, T?> result = new();

        var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Expected object, got {element.ValueKind}");
        }

        foreach (var propertyElement in element.EnumerateObject())
        {
            var typeId = JsonSerializer.Deserialize<TType>($"\"{propertyElement.Name}\"", options);

            if (typeId != null)
            {
                var inputType = GetValueType(typeId, propertyElement.Value, options)
                    .OrThrow(() => new JsonException($"Unknown type id: {typeId}"));

                if (
                    element.Deserialize(
                        inputType,
                        CreateValueOptions(options, typeId, inputType, propertyElement.Value)
                    )
                    is T value
                )
                {
                    result.Add(typeId, value);
                }
                else
                {
                    result.Add(typeId, default);
                }
            }
            else
            {
                throw new JsonException(
                    $"Unable to convert {propertyElement.Name} to type {typeof(TType)}"
                );
            }
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyDictionary<TType, T?> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (var entry in value.OrderBy(kvp => kvp.Key.ToString()))
        {
            var keyString = JsonSerializer.Serialize(entry.Key, options).Trim('"');

            if (entry.Value != null)
            {
                writer.WritePropertyName(keyString);
                JsonSerializer.Serialize(
                    writer,
                    entry.Value,
                    entry.Value.GetType(),
                    CreateValueOptions(options, entry.Key, entry.Value.GetType(), entry.Value)
                );
            }
            else
            {
                writer.WriteNull(keyString);
            }
        }

        writer.WriteEndObject();
    }

    protected abstract Optional<Type> GetValueType(
        TType type,
        JsonElement data,
        JsonSerializerOptions options
    );

    protected virtual JsonSerializerOptions CreateValueOptions(
        JsonSerializerOptions options,
        TType discriminator,
        Type valueType,
        JsonElement data
    )
    {
        return CreateValueOptions(options, discriminator, valueType);
    }

    protected virtual JsonSerializerOptions CreateValueOptions(
        JsonSerializerOptions options,
        TType discriminator,
        Type valueType,
        T value
    )
    {
        return CreateValueOptions(options, discriminator, valueType);
    }

    protected virtual JsonSerializerOptions CreateValueOptions(
        JsonSerializerOptions options,
        TType discriminator,
        Type valueType
    )
    {
        return options;
    }
}
