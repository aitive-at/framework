using System.Diagnostics.CodeAnalysis;
using Aitive.Framework.Json.Converters;

namespace Aitive.Framework.Versioning.Json;

public sealed class SemVersionJsonConverter : StringProxyJsonConverter<SemVersion>
{
    protected override bool TryParse(string value, [NotNullWhen(true)] out SemVersion? parsedValue)
    {
        return SemVersion.TryParse(value, SemVersionStyles.Any, out parsedValue);
    }

    protected override string ToString(SemVersion value)
    {
        return value.ToString();
    }
}
