using System.Text;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Defines an attribute specification for source generator use.
/// </summary>
public sealed class AttributeDefinition(string name)
{
    public string Name { get; } = name.EndsWith("Attribute") ? name : name + "Attribute";
    public string? Namespace { get; private set; }

    public string FullName => Namespace != null ? $"{Namespace}.{Name}" : Name;

    public string Filename => FullName + ".g.cs";

    public AttributeTargets Targets { get; private set; } = AttributeTargets.All;
    public bool AllowMultiple { get; private set; }
    public bool Inherited { get; private set; } = true;
    public IReadOnlyList<AttributeConstructorDefinition> Constructors => _constructors;
    public IReadOnlyList<AttributePropertyDefinition> Properties => _properties;
    public IReadOnlyList<AttributeTypeParameterDefinition> TypeParameters => _typeParams;

    private readonly List<AttributeConstructorDefinition> _constructors = new();
    private readonly List<AttributePropertyDefinition> _properties = new();
    private readonly List<AttributeTypeParameterDefinition> _typeParams = new();

    // For backwards compatibility - tracks if using legacy single-constructor mode
    private AttributeConstructorDefinition? _defaultCtor;

    // Fluent configuration
    public AttributeDefinition WithNamespace(string ns)
    {
        Namespace = ns;
        return this;
    }

    public AttributeDefinition WithTargets(AttributeTargets targets)
    {
        Targets = targets;
        return this;
    }

    public AttributeDefinition WithAllowMultiple(bool allow = true)
    {
        AllowMultiple = allow;
        return this;
    }

    public AttributeDefinition WithInherited(bool inherited)
    {
        Inherited = inherited;
        return this;
    }

    /// <summary>
    /// Adds a type parameter with optional constraints.
    /// </summary>
    public AttributeDefinition WithTypeParameter(
        string name,
        Action<AttributeTypeParameterDefinition>? configure = null
    )
    {
        var spec = new AttributeTypeParameterDefinition(name);
        configure?.Invoke(spec);
        _typeParams.Add(spec);
        return this;
    }

    /// <summary>
    /// Adds a constructor. Use this for multiple constructor support.
    /// </summary>
    public AttributeDefinition WithConstructor(
        string? name,
        Action<AttributeConstructorDefinition> configure
    )
    {
        var ctor = new AttributeConstructorDefinition(name);
        configure(ctor);
        _constructors.Add(ctor);
        return this;
    }

    /// <summary>
    /// Adds a parameter to the default constructor (legacy single-constructor mode).
    /// </summary>
    public AttributeDefinition WithParameter(
        string name,
        AttributeParameterType type,
        object? defaultValue = null,
        bool isParams = false
    )
    {
        EnsureDefaultCtor();
        _defaultCtor!.WithParameter(name, type, defaultValue, isParams);
        return this;
    }

    public AttributeDefinition WithProperty(
        string name,
        AttributeParameterType type,
        object? defaultValue = null
    )
    {
        _properties.Add(new AttributePropertyDefinition(name, type, defaultValue));
        return this;
    }

    /// <summary>
    /// Creates a reader for parsing AttributeData instances.
    /// </summary>
    public AttributeValuesReader CreateReader() => new(this);

    public override string ToString()
    {
        return Generate(this);
    }

    private static string Generate(AttributeDefinition definition)
    {
        var sb = new StringBuilder();

        // Namespace
        if (definition.Namespace != null)
        {
            sb.AppendLine($"namespace {definition.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine("[global::Microsoft.CodeAnalysis.EmbeddedAttribute]");

        // AttributeUsage
        var targets = FormatTargets(definition.Targets);
        sb.AppendLine(
            $"[global::System.AttributeUsage({targets}, AllowMultiple = {BoolLiteral(definition.AllowMultiple)}, Inherited = {BoolLiteral(definition.Inherited)})]"
        );

        // Class declaration with type parameters
        var typeParamNames =
            definition.TypeParameters.Count > 0
                ? $"<{string.Join(", ", definition.TypeParameters.Select(t => t.Name))}>"
                : "";
        sb.Append(
            $"internal sealed class {definition.Name}{typeParamNames} : global::System.Attribute"
        );

        // Type parameter constraints
        var constraints = definition
            .TypeParameters.Select(t => t.GenerateConstraintClause())
            .Where(c => c != null)
            .ToList();

        if (constraints.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"    {string.Join(" ", constraints)}");
        }

        sb.AppendLine();
        sb.AppendLine("{");

        // Collect all unique properties from all constructors
        var allCtorParams = definition
            .Constructors.SelectMany(c => c.Parameters)
            .GroupBy(p => ToPascalCase(p.Name))
            .Select(g => g.First())
            .ToList();

        // Constructor parameters as properties (auto-implemented with private set for multi-ctor)
        var needsPrivateSet = definition.Constructors.Count > 1;
        foreach (var param in allCtorParams)
        {
            var propName = ToPascalCase(param.Name);
            var setter = needsPrivateSet ? " private set;" : "";
            sb.AppendLine(
                $"    internal {param.AttributeParameterType.ToCSharpString()} {propName} {{ get;{setter} }}"
            );
        }

        // Named properties
        foreach (var prop in definition.Properties)
        {
            var defaultPart =
                prop.DefaultValue != null
                    ? $" = {FormatValue(prop.DefaultValue, prop.AttributeParameterType)};"
                    : "";
            sb.AppendLine(
                $"    internal {prop.AttributeParameterType.ToCSharpString()} {prop.Name} {{ get; set; }}{defaultPart}"
            );
        }

        // Constructors
        foreach (var ctor in definition.Constructors)
        {
            if (ctor.Parameters.Count > 0 || definition.Constructors.Count == 1)
            {
                if (allCtorParams.Count > 0 || definition.Properties.Count > 0)
                {
                    sb.AppendLine();
                }

                var ctorParams = string.Join(", ", ctor.Parameters.Select(FormatParameter));
                sb.AppendLine($"    internal {definition.Name}({ctorParams})");
                sb.AppendLine("    {");
                foreach (var param in ctor.Parameters)
                {
                    var propName = ToPascalCase(param.Name);
                    sb.AppendLine($"        {propName} = {param.Name};");
                }
                sb.AppendLine("    }");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string FormatParameter(AttributeParameterDefinition param)
    {
        var prefix = param.IsParams ? "params " : "";
        var suffix = param.HasDefault
            ? $" = {FormatValue(param.DefaultValue, param.AttributeParameterType)}"
            : "";
        return $"{prefix}{param.AttributeParameterType.ToCSharpString()} {param.Name}{suffix}";
    }

    private static string FormatValue(object? value, AttributeParameterType type)
    {
        if (value == null)
        {
            return type is EnumAttributeParameterType ? "default" : "null";
        }

        return value switch
        {
            string s => $"\"{Escape(s)}\"",
            char c => $"'{c}'",
            bool b => BoolLiteral(b),
            int or long or byte or short => value.ToString()!,
            float f => $"{f}f",
            double d => $"{d}d",
            Enum e => $"{type.ToCSharpString().TrimEnd('?')}.{e}",
            Type t => $"typeof({t.FullName})",
            _ => value.ToString()!,
        };
    }

    private static string FormatTargets(AttributeTargets targets)
    {
        if (targets == AttributeTargets.All)
        {
            return "global::System.AttributeTargets.All";
        }

        var parts = Enum.GetValues(typeof(AttributeTargets))
            .Cast<AttributeTargets>()
            .Where(t => t != AttributeTargets.All && targets.HasFlag(t))
            .Select(t => $"global::System.AttributeTargets.{t}");

        return string.Join(" | ", parts);
    }

    private static string BoolLiteral(bool b) => b ? "true" : "false";

    private static string Escape(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");

    private static string ToPascalCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);

    private void EnsureDefaultCtor()
    {
        if (_defaultCtor != null)
        {
            return;
        }
        _defaultCtor = new AttributeConstructorDefinition(null);
        _constructors.Add(_defaultCtor);
    }
}
