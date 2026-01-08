using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Extension methods for typed attribute reading.
/// </summary>
public static class AttributeDefinitionExtensions
{
    extension<T>(AttributeDefinition definition)
        where T : class
    {
        /// <summary>
        /// Creates a typed reader for the spec class T.
        /// </summary>
        public TypedAttributeReader<T> CreateTypedReader() => new(definition);

        /// <summary>
        /// Reads AttributeData directly into an instance of T.
        /// </summary>
        public T ReadAs(AttributeData data) => new TypedAttributeReader<T>(definition).Read(data);
    }
}
