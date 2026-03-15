using System.Text.Json;
using System.Text.Json.Nodes;
using Aitive.Framework.Cryptography.Hashing;
using Aitive.Framework.Cryptography.Hashing.Algorithms;
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
                    Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
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
                    Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
                ) ?? throw new InvalidOperationException();
        }

        public static T Read(JsonElement json, JsonSerializerOptions? options = null)
        {
            return json.Deserialize<T>(
                    Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
                ) ?? throw new InvalidOperationException();
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
                .DeserializeAsync<T>(
                    stream,
                    Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
                )
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
                Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
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
                Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
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
                    Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
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
                Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
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
                Globals.ResolveValueOrRegistered<JsonSerializerOptions>(options)
            );
        }

        public Sha256Value HashJsonToSha256(
            IHashProvider<Sha256Value>? hashProvider,
            JsonSerializerOptions? options = null,
            bool useRuntimeType = true
        )
        {
            using var hashBuilder = (
                Globals.ResolveValueOrRegistered(hashProvider)
            ).CreateBuilder();

            hashBuilder.Write(value.ToJsonString(options, useRuntimeType));

            return hashBuilder.Build();
        }
    }
}
