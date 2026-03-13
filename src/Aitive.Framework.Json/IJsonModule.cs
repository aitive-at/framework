using System.Text.Json;

namespace Aitive.Framework.Json;

public interface IJsonModule
{
    void Configure(JsonSerializerOptions options);
}
