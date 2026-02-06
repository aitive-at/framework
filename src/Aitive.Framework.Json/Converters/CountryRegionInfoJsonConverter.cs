using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Aitive.Framework.Json.Converters;

public sealed class CountryRegionInfoJsonConverter : StringProxyJsonConverter<RegionInfo>
{
    protected override bool TryParse(string value, [NotNullWhen(true)] out RegionInfo? parsedValue)
    {
        try
        {
            parsedValue = new RegionInfo(value);
            return true;
        }
        catch (ArgumentException)
        {
            parsedValue = null;
            return false;
        }
    }

    protected override string ToString(RegionInfo value)
    {
        return value.TwoLetterISORegionName;
    }
}
