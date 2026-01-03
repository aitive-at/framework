using System.Reflection;
using System.Text;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

public static class ParameterInfoExtensions
{
    extension(ParameterInfo parameterInfo)
    {
        public string Declaration
        {
            get
            {
                var parts = new List<string>();

                // Modifiers
                if (parameterInfo.IsIn)
                {
                    parts.Add("in");
                }
                else if (parameterInfo.IsOut)
                {
                    parts.Add("out");
                }
                else if (parameterInfo.ParameterType.IsByRef)
                {
                    parts.Add("ref");
                }

                // Type (strip & from ByRef types)
                var type = parameterInfo.ParameterType.IsByRef
                    ? parameterInfo.ParameterType.GetElementType()!
                    : parameterInfo.ParameterType;

                parts.Add(type.DeclarationName);

                // Name
                parts.Add(parameterInfo.Name ?? "");

                var result = string.Join(" ", parts);

                // Default value
                var defaultValue = parameterInfo.DefaultValueString;
                if (defaultValue != null)
                {
                    result += $" = {defaultValue}";
                }

                return result;
            }
        }

        public string? DefaultValueString
        {
            get
            {
                if (!parameterInfo.HasDefaultValue)
                {
                    return null;
                }

                var value = parameterInfo.DefaultValue;
                var paramType = parameterInfo.ParameterType;

                // null (for reference types and nullable value types)
                if (value is null)
                {
                    return "null";
                }

                // DBNull means no default value was actually set (shouldn't hit this if HasDefaultValue is true, but safety)
                if (value is DBNull)
                {
                    return null;
                }

                // String
                if (value is string s)
                {
                    return $"\"{EscapeString(s)}\"";
                }

                // Char
                if (value is char c)
                {
                    return $"'{EscapeChar(c)}'";
                }

                // Boolean
                if (value is bool b)
                {
                    return b ? "true" : "false";
                }

                // Enum
                if (paramType.IsEnum)
                {
                    var enumName = Enum.GetName(paramType, value);
                    if (enumName != null)
                    {
                        return $"{paramType.DeclarationName}.{enumName}";
                    }

                    // Flags or unknown numeric value - cast it
                    return $"({paramType.DeclarationName}){value}";
                }

                // Numeric types
                if (value is float f)
                {
                    return float.IsPositiveInfinity(f) ? "float.PositiveInfinity"
                        : float.IsNegativeInfinity(f) ? "float.NegativeInfinity"
                        : float.IsNaN(f) ? "float.NaN"
                        : $"{f}f";
                }

                if (value is double d)
                {
                    return double.IsPositiveInfinity(d) ? "double.PositiveInfinity"
                        : double.IsNegativeInfinity(d) ? "double.NegativeInfinity"
                        : double.IsNaN(d) ? "double.NaN"
                        : $"{d}d";
                }

                if (value is decimal m)
                {
                    return $"{m}m";
                }

                if (value is long l)
                {
                    return $"{l}L";
                }

                if (value is ulong ul)
                {
                    return $"{ul}UL";
                }

                if (value is uint ui)
                {
                    return $"{ui}U";
                }

                // default(T) for value types that are their default
                if (paramType.IsValueType && value.Equals(Activator.CreateInstance(paramType)))
                {
                    return $"default";
                }

                // Other numeric types (int, byte, short, etc.)
                return value.ToString();
            }
        }
    }

    private static string EscapeString(string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s)
        {
            sb.Append(EscapeChar(c, forString: true));
        }

        return sb.ToString();
    }

    private static string EscapeChar(char c, bool forString = false)
    {
        return c switch
        {
            '\'' when !forString => @"\'",
            '\"' when forString => @"\""",
            '\\' => @"\\",
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\f' => @"\f",
            '\n' => @"\n",
            '\r' => @"\r",
            '\t' => @"\t",
            '\v' => @"\v",
            _ when char.IsControl(c) => $@"\u{(int)c:X4}",
            _ => c.ToString(),
        };
    }
}
