using System.Text.Json;
using Aitive.Framework.Json;
using Aitive.Framework.Patterns;

namespace Aitive.Framework.Orleans.Surrogates;

[GenerateSerializer]
public struct JsonSurrogate<T>
    where T : ITypedJsonDocument<T>
{
    [Id(0)]
    public string Json { get; set; }
}

public abstract class JsonSurrogateConverter<T> : IConverter<T, JsonSurrogate<T>>
    where T : ITypedJsonDocument<T>
{
    public T ConvertFromSurrogate(in JsonSurrogate<T> surrogate)
    {
        return JsonSerializer.Deserialize<T>(
                surrogate.Json,
                Globals.Resolve<JsonSerializerOptions>()
            ) ?? throw new InvalidOperationException();
    }

    public JsonSurrogate<T> ConvertToSurrogate(in T value)
    {
        return new JsonSurrogate<T>() { Json = value.ToJsonString() };
    }
}
