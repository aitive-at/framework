using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aitive.Framework.Json.Converters;

public class SubTypeJsonConverter<T> : JsonConverter<T>
{
    private readonly Type _subType;

    public SubTypeJsonConverter(Type subType)
    {
        _subType = subType;
    }

    public override T? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return (T?)
            JsonSerializer.Deserialize(
                ref reader,
                _subType,
                options.WithoutConverterInstance(this)
            );
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _subType, options.WithoutConverterInstance(this));
    }
}
