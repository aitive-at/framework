using Aitive.Framework.SourceGenerators;
using Aitive.Framework.SourceGenerators.Framework;
using Aitive.Framework.SourceGenerators.Framework.Dom;
using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Aitive.Framework.SourceGenerators.Framework.Logging;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Generators.OrleansSurrogate;

[GeneratedAttribute(Namespace = WellKnownNamespaces.GeneratedCodeNamespace)]
public sealed class OrleansSurrogateAttribute
{
    public INamedTypeSymbol TargetType { get; }

    public OrleansSurrogateAttribute(INamedTypeSymbol targetType)
    {
        TargetType = targetType;
    }
}

public record SurrogatePropertyModel(
    string Name, // PascalCase name for the surrogate property
    string TypeGlobalName, // global:: prefixed for unambiguous references
    bool IsConstructorParam,
    int ConstructorParamIndex,
    string? ConstructorParamName, // Original casing of the ctor param (for named args)
    string? MatchingPropertyName, // Property name on source type to read from
    bool HasMatchingReadableProperty, // Can we read this back via a property?
    bool IsInitOnly, // init-only setter (use object initializer syntax)
    bool IsNullable
);

public record SurrogateModel(
    TypeDeclaration SurrogateDeclaration, // The partial struct the user declared
    string SurrogateName, // e.g. "ModelIdSurrogate"
    string TargetTypeName, // Short name of the external type
    string TargetTypeGlobalName, // global::Namespace.Type
    IReadOnlyList<SurrogatePropertyModel> AllProperties,
    IReadOnlyList<SurrogatePropertyModel> ConstructorProperties,
    IReadOnlyList<SurrogatePropertyModel> InitOnlyProperties,
    IReadOnlyList<SurrogatePropertyModel> MutableProperties,
    string ConverterName, // Name for the converter class
    bool IsGeneric,
    string? TypeParameters, // "<T1, T2>" or null
    string? TypeParameterConstraints, // "where T1 : class where T2 : struct" or null
    bool HasErrors
);

[Generator]
public class OrleansSurrogateGenerator : AttributedTypeSourceGenerator<OrleansSurrogateAttribute>
{
    protected override bool OnGenerate(
        OrleansSurrogateAttribute attribute,
        GeneratorAttributeSyntaxContext input,
        SourceWriter writer,
        ILogWriter log
    )
    {
        var surrogateSymbol = (INamedTypeSymbol)input.TargetSymbol;
        var targetType = attribute.TargetType;

        var model = AnalyzeForSurrogate(surrogateSymbol, targetType, log);

        if (model.HasErrors)
        {
            return false;
        }

        writer.WriteLineWithoutIndentation(RenderTemplate("OrleansSurrogate", model));
        return true;
    }

    private SurrogateModel AnalyzeForSurrogate(
        INamedTypeSymbol surrogateSymbol,
        INamedTypeSymbol targetType,
        ILogWriter log
    )
    {
        var constructor = SelectConstructor(targetType);

        if (constructor is null)
        {
            log.Error($"No suitable public constructor found on {targetType.ToDisplayString()}");
            return CreateErrorModel(surrogateSymbol, targetType);
        }

        var allReadableProperties = GetAllReadableProperties(targetType);
        var allSettableProperties = GetAllSettableProperties(targetType);

        var properties = new List<SurrogatePropertyModel>();
        var coveredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Constructor parameters
        for (int i = 0; i < constructor.Parameters.Length; i++)
        {
            var param = constructor.Parameters[i];
            var matchingProp = FindMatchingProperty(param, allReadableProperties);

            if (matchingProp is null)
            {
                log.Warning(
                    $"Constructor parameter '{param.Name}' on {targetType.Name} has no matching "
                        + "readable property — surrogate cannot roundtrip this value from the source type. "
                        + $"The surrogate property will default to default({param.Type.ToDisplayString()})."
                );
            }

            var propName = matchingProp?.Name ?? ToPascalCase(param.Name);

            properties.Add(
                new SurrogatePropertyModel(
                    Name: propName,
                    TypeGlobalName: param.Type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat
                    ),
                    IsConstructorParam: true,
                    ConstructorParamIndex: i,
                    ConstructorParamName: param.Name,
                    MatchingPropertyName: matchingProp?.Name,
                    HasMatchingReadableProperty: matchingProp is not null,
                    IsInitOnly: false,
                    IsNullable: IsNullableType(param.Type)
                )
            );

            coveredNames.Add(propName);
        }

        // 2. Settable/init-only properties not covered by constructor
        foreach (var prop in allSettableProperties)
        {
            if (coveredNames.Contains(prop.Name))
            {
                continue;
            }

            var canRead =
                prop.GetMethod is not null
                && prop.GetMethod.DeclaredAccessibility
                    is Accessibility.Public
                        or Accessibility.Internal;

            if (!canRead)
            {
                log.Warning(
                    $"Property '{prop.Name}' on {targetType.Name} is settable but not readable — skipping."
                );
                continue;
            }

            var isInit = IsInitOnly(prop);

            properties.Add(
                new SurrogatePropertyModel(
                    Name: prop.Name,
                    TypeGlobalName: prop.Type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat
                    ),
                    IsConstructorParam: false,
                    ConstructorParamIndex: -1,
                    ConstructorParamName: null,
                    MatchingPropertyName: prop.Name,
                    HasMatchingReadableProperty: true,
                    IsInitOnly: isInit,
                    IsNullable: IsNullableType(prop.Type)
                )
            );

            coveredNames.Add(prop.Name);
        }

        // 3. Informational warnings for readable-only properties we cannot roundtrip
        foreach (var prop in allReadableProperties)
        {
            if (coveredNames.Contains(prop.Name))
            {
                continue;
            }

            if (prop.IsImplicitlyDeclared || prop.IsStatic)
            {
                continue;
            }

            log.Info(
                $"Property '{prop.Name}' on {targetType.Name} is readable but not settable "
                    + "and not a constructor parameter — it will not be included in the surrogate."
            );
        }

        // Generic type info — derived from the TARGET type
        var isGeneric = targetType.IsGenericType;
        string? typeParams = null;
        string? typeConstraints = null;

        if (isGeneric)
        {
            typeParams = BuildTypeParameterList(targetType);
            typeConstraints = BuildTypeConstraints(targetType);
        }

        var ctorProps = properties.Where(p => p.IsConstructorParam).ToList();
        var initProps = properties.Where(p => !p.IsConstructorParam && p.IsInitOnly).ToList();
        var mutableProps = properties.Where(p => !p.IsConstructorParam && !p.IsInitOnly).ToList();

        // Derive converter name from surrogate name
        var converterName = surrogateSymbol.Name.EndsWith("Surrogate")
            ? surrogateSymbol.Name[..^"Surrogate".Length] + "SurrogateConverter"
            : surrogateSymbol.Name + "Converter";

        return new SurrogateModel(
            SurrogateDeclaration: surrogateSymbol.TypeDeclaration,
            SurrogateName: isGeneric ? $"{surrogateSymbol.Name}{typeParams}" : surrogateSymbol.Name,
            TargetTypeName: isGeneric ? $"{targetType.Name}{typeParams}" : targetType.Name,
            TargetTypeGlobalName: targetType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            AllProperties: properties,
            ConstructorProperties: ctorProps,
            InitOnlyProperties: initProps,
            MutableProperties: mutableProps,
            ConverterName: isGeneric ? $"{converterName}{typeParams}" : converterName,
            IsGeneric: isGeneric,
            TypeParameters: typeParams,
            TypeParameterConstraints: typeConstraints,
            HasErrors: false
        );
    }

    // ---- Constructor Selection ----

    private static IMethodSymbol? SelectConstructor(INamedTypeSymbol type)
    {
        var publicCtors = type
            .Constructors.Where(c => !c.IsStatic)
            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        if (publicCtors.Count == 0)
        {
            return null;
        }

        // 1. Prefer constructor marked with a known attribute
        var attributed = publicCtors.FirstOrDefault(c =>
            c.GetAttributes()
                .Any(a =>
                    a.AttributeClass?.Name
                        is "ActivatorUtilitiesConstructorAttribute"
                            or "SurrogateConstructorAttribute"
                            or "JsonConstructorAttribute"
                )
        );

        if (attributed is not null)
        {
            return attributed;
        }

        // 2. For record types with a primary constructor, prefer it
        if (type.IsRecord)
        {
            var primaryCtor = FindPrimaryConstructor(type, publicCtors);
            if (primaryCtor is not null)
            {
                return primaryCtor;
            }
        }

        // 3. Fall back to the constructor with the most parameters
        //    Tie-break: prefer the one with the most matching readable properties
        var allReadable = GetAllReadableProperties(type);

        return publicCtors
            .OrderByDescending(c => c.Parameters.Length)
            .ThenByDescending(c =>
                c.Parameters.Count(p => FindMatchingProperty(p, allReadable) is not null)
            )
            .First();
    }

    private static IMethodSymbol? FindPrimaryConstructor(
        INamedTypeSymbol recordType,
        List<IMethodSymbol> constructors
    )
    {
        // A record's primary constructor parameters correspond 1:1 with
        // compiler-generated properties that are init-only or get-only.
        var declaredProps = recordType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer)
            .ToList();

        foreach (var ctor in constructors.OrderByDescending(c => c.Parameters.Length))
        {
            var allMatch = ctor.Parameters.All(param =>
                declaredProps.Any(prop =>
                    string.Equals(prop.Name, param.Name, StringComparison.OrdinalIgnoreCase)
                    && SymbolEqualityComparer.Default.Equals(prop.Type, param.Type)
                )
            );

            if (allMatch && ctor.Parameters.Length > 0)
            {
                return ctor;
            }
        }

        return null;
    }

    // ---- Property Collection (with inheritance) ----

    private static List<IPropertySymbol> GetAllReadableProperties(INamedTypeSymbol type)
    {
        var result = new List<IPropertySymbol>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = type;

        while (current is not null)
        {
            foreach (var prop in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.IsStatic || prop.IsIndexer || prop.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (prop.GetMethod is null)
                {
                    continue;
                }

                if (
                    prop.GetMethod.DeclaredAccessibility
                    is not (Accessibility.Public or Accessibility.Internal)
                )
                {
                    continue;
                }

                // Only take the most-derived version (first seen wins)
                if (!seen.Add(prop.Name))
                {
                    continue;
                }

                result.Add(prop);
            }

            current = current.BaseType;
        }

        return result;
    }

    private static List<IPropertySymbol> GetAllSettableProperties(INamedTypeSymbol type)
    {
        var result = new List<IPropertySymbol>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = type;

        while (current is not null)
        {
            foreach (var prop in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.IsStatic || prop.IsIndexer || prop.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (prop.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (!IsSettable(prop))
                {
                    continue;
                }

                if (!seen.Add(prop.Name))
                {
                    continue;
                }

                result.Add(prop);
            }

            current = current.BaseType;
        }

        return result;
    }

    // ---- Property Matching ----

    private static IPropertySymbol? FindMatchingProperty(
        IParameterSymbol constructorParam,
        List<IPropertySymbol> readableProperties
    )
    {
        // 1. Exact name + type match (case-insensitive name)
        var exact = readableProperties.FirstOrDefault(p =>
            string.Equals(p.Name, constructorParam.Name, StringComparison.OrdinalIgnoreCase)
            && SymbolEqualityComparer.Default.Equals(p.Type, constructorParam.Type)
        );

        if (exact is not null)
        {
            return exact;
        }

        // 2. Name match with nullability-agnostic type comparison
        var nameMatch = readableProperties.FirstOrDefault(p =>
            string.Equals(p.Name, constructorParam.Name, StringComparison.OrdinalIgnoreCase)
            && SymbolEqualityComparer.Default.Equals(
                p.Type.WithNullableAnnotation(NullableAnnotation.None),
                constructorParam.Type.WithNullableAnnotation(NullableAnnotation.None)
            )
        );

        if (nameMatch is not null)
        {
            return nameMatch;
        }

        // 3. Common naming conventions: _name, m_name → Name
        var strippedParamName = StripFieldPrefix(constructorParam.Name);
        if (strippedParamName != constructorParam.Name)
        {
            var prefixMatch = readableProperties.FirstOrDefault(p =>
                string.Equals(p.Name, strippedParamName, StringComparison.OrdinalIgnoreCase)
                && SymbolEqualityComparer.Default.Equals(
                    p.Type.WithNullableAnnotation(NullableAnnotation.None),
                    constructorParam.Type.WithNullableAnnotation(NullableAnnotation.None)
                )
            );

            if (prefixMatch is not null)
            {
                return prefixMatch;
            }
        }

        return null;
    }

    // ---- Generic Type Handling ----

    private static string BuildTypeParameterList(INamedTypeSymbol type)
    {
        var names = type.TypeParameters.Select(tp => tp.Name);
        return $"<{string.Join(", ", names)}>";
    }

    private static string? BuildTypeConstraints(INamedTypeSymbol type)
    {
        var clauses = new List<string>();

        foreach (var tp in type.TypeParameters)
        {
            var constraints = new List<string>();

            // class/struct/unmanaged/notnull must come first
            if (tp.HasReferenceTypeConstraint)
            {
                constraints.Add(
                    tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                        ? "class?"
                        : "class"
                );
            }
            else if (tp.HasValueTypeConstraint)
            {
                constraints.Add(tp.HasUnmanagedTypeConstraint ? "unmanaged" : "struct");
            }
            else if (tp.HasNotNullConstraint)
            {
                constraints.Add("notnull");
            }

            // Type constraints
            foreach (var ct in tp.ConstraintTypes)
            {
                constraints.Add(ct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            // new() must come last, and is implied by struct/unmanaged
            if (tp.HasConstructorConstraint && !tp.HasValueTypeConstraint)
            {
                constraints.Add("new()");
            }

            if (constraints.Count > 0)
            {
                clauses.Add($"where {tp.Name} : {string.Join(", ", constraints)}");
            }
        }

        return clauses.Count > 0 ? string.Join(" ", clauses) : null;
    }

    // ---- Utilities ----

    private static bool IsSettable(IPropertySymbol property)
    {
        if (property.SetMethod is null)
        {
            return false;
        }

        return property.SetMethod.DeclaredAccessibility
            is Accessibility.Public
                or Accessibility.Internal;
    }

    private static bool IsInitOnly(IPropertySymbol property)
    {
        return property.SetMethod?.IsInitOnly ?? false;
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        if (
            type is INamedTypeSymbol { IsGenericType: true } named
            && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
        )
        {
            return true;
        }

        return false;
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var stripped = StripFieldPrefix(name);
        return char.ToUpperInvariant(stripped[0]) + stripped[1..];
    }

    private static string StripFieldPrefix(string name)
    {
        // _name → name
        if (name.StartsWith("_") && name.Length > 1)
        {
            return name[1..];
        }

        // m_name → name
        if (name.StartsWith("m_") && name.Length > 2)
        {
            return name[2..];
        }

        return name;
    }

    private static SurrogateModel CreateErrorModel(
        INamedTypeSymbol surrogateSymbol,
        INamedTypeSymbol targetType
    )
    {
        return new SurrogateModel(
            SurrogateDeclaration: surrogateSymbol.TypeDeclaration,
            SurrogateName: surrogateSymbol.Name,
            TargetTypeName: targetType.Name,
            TargetTypeGlobalName: targetType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            AllProperties: [],
            ConstructorProperties: [],
            InitOnlyProperties: [],
            MutableProperties: [],
            ConverterName: surrogateSymbol.Name + "Converter",
            IsGeneric: false,
            TypeParameters: null,
            TypeParameterConstraints: null,
            HasErrors: true
        );
    }
}
