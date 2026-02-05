using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Functional;
using Aitive.Framework.Reflection;

namespace Aitive.Framework.Json.Converters;

public class PolymorphicResolverJsonConverter<TType, T>(ITypeResolver<TType> typeResolver)
    : PolymorphicJsonConverter<TType, T>
    where TType : notnull
    where T : notnull
{
    protected override Optional<Type> GetValueType(
        TType type,
        JsonElement data,
        JsonSerializerOptions options
    )
    {
        return typeResolver.Resolve(type);
    }

    protected override Optional<TType> GetTypeId(T value, JsonSerializerOptions options)
    {
        return typeResolver.Resolve(value.GetType());
    }
}

public abstract class PolymorphicJsonConverter<TType, T>(
    bool flatten = false,
    string typeDiscriminatorName = "type",
    string valueName = "value"
) : JsonConverter<T>
    where TType : notnull
{
    public override T? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Expected object, got {element.ValueKind}");
        }

        if (element.TryGetProperty(typeDiscriminatorName, out var typeIdElement))
        {
            var typeId =
                typeIdElement.Deserialize<TType>(options)
                ?? throw new JsonException(
                    $"Invalid {typeDiscriminatorName} value: {typeIdElement.GetRawText()}"
                );

            var inputType = GetValueType(typeId, element, options)
                .OrThrow(() => new JsonException($"Unknown {typeDiscriminatorName}: {typeId}"));

            if (flatten)
            {
                return (T?)
                    element.Deserialize(
                        inputType,
                        CreateValueOptions(options, typeId, inputType, element)
                    );
            }

            if (element.TryGetProperty(valueName, out var valueElement))
            {
                return (T?)
                    valueElement.Deserialize(
                        inputType,
                        CreateValueOptions(options, typeId, inputType, element)
                    );
            }
        }

        throw new JsonException(
            $"Invalid polymorphic object, {typeDiscriminatorName} or value property missing"
        );
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var typeId = GetTypeId(value, options)
            .OrThrow(() => new JsonException($"Unknown type: {value.GetType()}"));

        writer.WriteStartObject();

        writer.WritePropertyName(typeDiscriminatorName);
        JsonSerializer.Serialize(writer, typeId, options);

        if (flatten)
        {
            var objectElement = JsonSerializer.SerializeToElement(
                value,
                value.GetType(),
                CreateValueOptions(options, typeId, value.GetType(), value)
            );

            foreach (var property in objectElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
        }
        else
        {
            writer.WritePropertyName(valueName);
            JsonSerializer.Serialize(
                writer,
                value,
                value.GetType(),
                CreateValueOptions(options, typeId, value.GetType(), value)
            );
        }

        writer.WriteEndObject();
    }

    protected abstract Optional<Type> GetValueType(
        TType type,
        JsonElement data,
        JsonSerializerOptions options
    );

    protected abstract Optional<TType> GetTypeId(T value, JsonSerializerOptions options);

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
        return options.WithoutConverterInstance(this);
    }
}
