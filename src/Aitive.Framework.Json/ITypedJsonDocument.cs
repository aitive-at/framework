using System.Text.Json;
using System.Text.Json.Nodes;
using Aitive.Framework.Patterns;

namespace Aitive.Framework.Json;

public interface ITypedJsonDocument<T>
    where T : ITypedJsonDocument<T> { }

public static class TypedJsonDocumentExtensions
{
    extension<T>(T)
        where T : ITypedJsonDocument<T>
    {
        public static T Read(Stream stream, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(
                    stream,
                    options ?? Globals.Resolve<JsonSerializerOptions>()
                ) ?? throw new InvalidOperationException();
        }

        public static T Read(string json, JsonSerializerOptions? options = null)
        {
            return Read<T>((ReadOnlySpan<char>)json, options);
        }

        public static T Read(ReadOnlySpan<char> text, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(
                    text,
                    options ?? Globals.Resolve<JsonSerializerOptions>()
                ) ?? throw new InvalidOperationException();
        }

        public static T Read(JsonElement json, JsonSerializerOptions? options = null)
        {
            return json.Deserialize<T>(options ?? Globals.Resolve<JsonSerializerOptions>())
                ?? throw new InvalidOperationException();
        }

        public static T Read(JsonNode node, JsonSerializerOptions? options = null)
        {
            return node.Deserialize<T>(options ?? Globals.Resolve<JsonSerializerOptions>())
                ?? throw new InvalidOperationException();
        }

        public static T ReadFile(string path, JsonSerializerOptions? options = null)
        {
            var text = File.ReadAllText(path);

            return Read<T>(text, options);
        }

        public static async Task<T> ReadAsync(Stream stream, JsonSerializerOptions? options = null)
        {
            var result = await JsonSerializer
                .DeserializeAsync<T>(stream, options ?? Globals.Resolve<JsonSerializerOptions>())
                .ConfigureAwait(false);
            return result ?? throw new InvalidOperationException();
        }
    }

    extension<T>(T value)
        where T : ITypedJsonDocument<T>
    {
        public void Write(
            Stream stream,
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            var type = useRuntimeType ? value.GetType() : typeof(T);

            JsonSerializer.Serialize(
                stream,
                value,
                type,
                options ?? Globals.Resolve<JsonSerializerOptions>()
            );
        }

        public Task WriteAsync(
            Stream stream,
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            var type = useRuntimeType ? value.GetType() : typeof(T);

            return JsonSerializer.SerializeAsync(
                stream,
                value,
                type,
                options ?? Globals.Resolve<JsonSerializerOptions>()
            );
        }

        public void WriteFile(
            string path,
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            using var stream = File.Open(path, FileMode.Create);

            value.Write(stream, options, useRuntimeType);
        }

        public JsonNode ToJsonNode(
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            var type = useRuntimeType ? value.GetType() : typeof(T);

            return JsonSerializer.SerializeToNode(
                    value,
                    type,
                    options ?? Globals.Resolve<JsonSerializerOptions>()
                ) ?? throw new InvalidOperationException();
        }

        public JsonElement ToJsonElement(
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            var type = useRuntimeType ? value.GetType() : typeof(T);

            return JsonSerializer.SerializeToElement(
                value,
                type,
                options ?? Globals.Resolve<JsonSerializerOptions>()
            );
        }

        public string ToJsonString(
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            var type = useRuntimeType ? value.GetType() : typeof(T);

            return JsonSerializer.Serialize(
                value,
                type,
                options ?? Globals.Resolve<JsonSerializerOptions>()
            );
        }
    }
}
