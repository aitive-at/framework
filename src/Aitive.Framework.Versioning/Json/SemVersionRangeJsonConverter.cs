using System.Diagnostics.CodeAnalysis;
using Aitive.Framework.Json.Converters;

namespace Aitive.Framework.Versioning.Json;

public sealed class SemVersionRangeJsonConverter : StringProxyJsonConverter<SemVersionRange>
{
    protected override bool TryParse(
        string value,
        [NotNullWhen(true)] out SemVersionRange? parsedValue
    )
    {
        return SemVersionRange.TryParse(value, SemVersionRangeOptions.Loose, out parsedValue);
    }

    protected override string ToString(SemVersionRange value)
    {
        return value.ToString();
    }
}
