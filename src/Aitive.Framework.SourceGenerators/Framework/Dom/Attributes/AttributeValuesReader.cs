using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Reads AttributeData and provides strongly-typed access to values.
/// </summary>
public sealed class AttributeValuesReader
{
    private readonly AttributeDefinition _definition;

    public AttributeValuesReader(AttributeDefinition definition) => _definition = definition;

    /// <summary>
    /// Reads an AttributeData instance and returns strongly-typed values.
    /// </summary>
    public AttributeValues Read(AttributeData data)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var typeParams = new Dictionary<string, ITypeSymbol?>(StringComparer.OrdinalIgnoreCase);

        // Read generic type arguments (e.g., [Handler<MyService, MyResult>])
        if (data.AttributeClass is { IsGenericType: true } attrClass)
        {
            var typeArgs = attrClass.TypeArguments;
            for (int i = 0; i < typeArgs.Length && i < _definition.TypeParameters.Count; i++)
            {
                typeParams[_definition.TypeParameters[i].Name] = typeArgs[i];
            }
        }

        // Find matching constructor based on argument count
        var ctorArgs = data.ConstructorArguments;
        var matchedCtor = FindMatchingConstructor(ctorArgs, data);

        // Read constructor arguments
        if (matchedCtor != null)
        {
            for (int i = 0; i < ctorArgs.Length && i < matchedCtor.Parameters.Count; i++)
            {
                var param = matchedCtor.Parameters[i];
                var propName = ToPascalCase(param.Name);
                values[propName] = ConvertValue(ctorArgs[i], param.AttributeParameterType);
            }

            // Apply defaults for missing constructor parameters
            for (int i = ctorArgs.Length; i < matchedCtor.Parameters.Count; i++)
            {
                var param = matchedCtor.Parameters[i];
                var propName = ToPascalCase(param.Name);
                if (!values.ContainsKey(propName))
                {
                    values[propName] = GetDefaultValue(
                        param.AttributeParameterType,
                        param.DefaultValue
                    );
                }
            }
        }

        // Read named arguments
        foreach (var namedArg in data.NamedArguments)
        {
            var prop = _definition.Properties.FirstOrDefault(p =>
                string.Equals(p.Name, namedArg.Key, StringComparison.OrdinalIgnoreCase)
            );
            if (prop != null)
            {
                values[namedArg.Key] = ConvertValue(namedArg.Value, prop.AttributeParameterType);
            }
        }

        // Apply defaults for missing properties
        foreach (var prop in _definition.Properties)
        {
            if (!values.ContainsKey(prop.Name))
            {
                values[prop.Name] = GetDefaultValue(prop.AttributeParameterType, prop.DefaultValue);
            }
        }

        return new AttributeValues(values, typeParams, matchedCtor?.Name);
    }

    private ConstructorDefinition? FindMatchingConstructor(
        ImmutableArray<TypedConstant> args,
        AttributeData data
    )
    {
        if (_definition.Constructors.Count == 0)
        {
            return null;
        }
        if (_definition.Constructors.Count == 1)
        {
            return _definition.Constructors[0];
        }

        // Use the actual constructor symbol from AttributeData to match by signature
        var attrCtor = data.AttributeConstructor;
        if (attrCtor != null)
        {
            var attrParams = attrCtor.Parameters;
            foreach (var ctor in _definition.Constructors)
            {
                if (
                    ctor.Parameters.Count == attrParams.Length
                    && MatchesSignature(ctor.Parameters, attrParams)
                )
                {
                    return ctor;
                }
            }
        }

        // Fallback: Find constructors where arg count fits
        var candidates = _definition
            .Constructors.Where(c =>
            {
                int required = c.Parameters.Count(p => !p.HasDefault);
                return required <= args.Length && args.Length <= c.Parameters.Count;
            })
            .ToList();

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        // Fall back to first candidate or first constructor
        return candidates.FirstOrDefault() ?? _definition.Constructors[0];
    }

    private static bool MatchesSignature(
        IReadOnlyList<ParameterDefinition> specParams,
        ImmutableArray<IParameterSymbol> attrParams
    )
    {
        for (int i = 0; i < specParams.Count; i++)
        {
            var specType = specParams[i].AttributeParameterType;
            var attrType = attrParams[i].Type;

            // Check if types match based on the spec's TypeRef kind
            if (!TypeRefMatchesSymbol(specType, attrType))
            {
                return false;
            }
        }
        return true;
    }

    private static bool TypeRefMatchesSymbol(AttributeParameterType type, ITypeSymbol typeSymbol)
    {
        return type switch
        {
            SymbolAttributeParameterType => typeSymbol.ToDisplayString() == "System.Type",
            EnumAttributeParameterType e => typeSymbol.ToDisplayString() == e.FullyQualifiedName,
            PrimitiveAttributeParameterType p => MatchesPrimitiveType(p, typeSymbol),
            ArrayAttributeParameterType a => typeSymbol is IArrayTypeSymbol arr
                && TypeRefMatchesSymbol(a.ElementAttributeParameterType, arr.ElementType),
            _ => false,
        };
    }

    private static bool MatchesPrimitiveType(
        PrimitiveAttributeParameterType p,
        ITypeSymbol typeSymbol
    )
    {
        var displayName = typeSymbol.ToDisplayString();
        var keyword = p.ToCSharpString().TrimEnd('?');
        return displayName == keyword
            || displayName == $"System.{char.ToUpper(keyword[0])}{keyword.Substring(1)}";
    }

    private object? ConvertValue(TypedConstant constant, AttributeParameterType type)
    {
        if (constant.Kind == TypedConstantKind.Error)
            return null;

        if (constant.IsNull)
            return null;

        return type switch
        {
            SymbolAttributeParameterType => constant.Value as INamedTypeSymbol,
            EnumAttributeParameterType => new EnumValue(
                constant.Type as INamedTypeSymbol,
                constant.Value
            ),
            ArrayAttributeParameterType arr => ConvertArray(
                constant.Values,
                arr.ElementAttributeParameterType
            ),
            _ => constant.Value,
        };
    }

    private object?[] ConvertArray(
        ImmutableArray<TypedConstant> values,
        AttributeParameterType elementType
    )
    {
        var result = new object?[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = ConvertValue(values[i], elementType);
        }
        return result;
    }

    private static object? GetDefaultValue(AttributeParameterType type, object? specifiedDefault)
    {
        if (specifiedDefault != null)
        {
            return specifiedDefault;
        }

        if (type.IsNullable)
        {
            return null;
        }

        return type.ClrType?.IsValueType == true ? Activator.CreateInstance(type.ClrType) : null;
    }

    private static string ToPascalCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);
}

/// <summary>
/// Represents an enum value from the target project.
/// </summary>
public sealed class EnumValue
{
    /// <summary>
    /// The enum type symbol from the target project.
    /// </summary>
    public INamedTypeSymbol? EnumType { get; }

    /// <summary>
    /// The underlying value (int, long, etc.).
    /// </summary>
    public object? UnderlyingValue { get; }

    /// <summary>
    /// The member name (e.g., "Value1").
    /// </summary>
    public string? MemberName =>
        EnumType
            ?.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, UnderlyingValue))
            ?.Name;

    /// <summary>
    /// The fully qualified type name.
    /// </summary>
    public string? FullTypeName =>
        EnumType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    internal EnumValue(INamedTypeSymbol? enumType, object? underlyingValue)
    {
        EnumType = enumType;
        UnderlyingValue = underlyingValue;
    }

    /// <summary>
    /// Maps to a CLR enum T if it exists in the generator (by member name).
    /// </summary>
    public T? ToEnum<T>()
        where T : struct, Enum
    {
        var memberName = MemberName;
        return memberName != null && Enum.TryParse<T>(memberName, out var result) ? result : null;
    }

    /// <summary>
    /// Maps to a CLR enum T by underlying value.
    /// </summary>
    public T ToEnumByValue<T>()
        where T : struct, Enum =>
        UnderlyingValue != null ? (T)Enum.ToObject(typeof(T), UnderlyingValue) : default;
}
