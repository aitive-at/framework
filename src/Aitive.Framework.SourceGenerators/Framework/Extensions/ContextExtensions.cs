using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Logging;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

internal static class ContextExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        public void GenerateSourceFilesForAttribute<T>(
            Func<T, GeneratorAttributeSyntaxContext, bool> predicate,
            Func<T, GeneratorAttributeSyntaxContext, SourceWriter, ILogWriter, bool> transform,
            Func<T, GeneratorAttributeSyntaxContext, string>? filenameProvider = null,
            AttributeDefinition? definition = null
        )
            where T : class
        {
            var finalDefinition = definition ?? AttributeDefinition.From<T>();
            var attributeReader = finalDefinition.CreateTypedReader<T>();
            Func<T, GeneratorAttributeSyntaxContext, string> finalFilenameProvider =
                filenameProvider
                ?? (
                    (
                        (attribute, syntaxContext) =>
                            ((ITypeSymbol)syntaxContext.TargetSymbol).CompanionFilename
                    )
                );

            var items = context
                .SyntaxProvider.ForAttributeWithMetadataName(
                    finalDefinition.FullName,
                    ((node, token) => true),
                    ((syntaxContext, token) => syntaxContext)
                )
                .Where(s => s.Attributes.Length == 1)
                .Select(
                    (s, token) =>
                    {
                        var attribute = attributeReader.Read(s.Attributes[0]);

                        return new
                        {
                            Attribute = attribute,
                            Context = s,
                            Definition = finalDefinition,
                        };
                    }
                );

            context.RegisterSourceOutput(
                items,
                (productionContext, item) =>
                {
                    var filename = finalFilenameProvider(item.Attribute, item.Context);
                    var logWriter = new SourceLogWriter();

                    var wasGenerated = false;

                    try
                    {
                        if (predicate(item.Attribute, item.Context))
                        {
                            wasGenerated = transform(
                                item.Attribute,
                                item.Context,
                                logWriter.InnerWriter,
                                logWriter
                            );
                        }
                        else
                        {
                            logWriter.Error(
                                $"Attribute {item.Definition.FullName} found on declaration: {item.Context.TargetSymbol.ToDisplayString()}, not supported"
                            );
                        }

                        if (wasGenerated || logWriter.ErrorCount > 0)
                        {
                            productionContext.AddSource(filename, logWriter.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logWriter.Error(ex);
                        productionContext.AddSource(filename, logWriter.ToString());
                    }
                }
            );
        }

        public void AddSourceFiles(IncrementalValuesProvider<SourceFile?> sourceFiles)
        {
            context.RegisterSourceOutput(
                sourceFiles.Where(o => o != null),
                (productionContext, sourceFile) => sourceFile?.AddToOutput(productionContext)
            );
        }

        public void RegisterPostInitializationOutput(
            string filename,
            Action<SourceWriter, ILogWriter> writingAction,
            Action<
                IncrementalGeneratorPostInitializationContext,
                ILogWriter
            >? postInitializationAction = null
        )
        {
            context.RegisterPostInitializationOutput(postContext =>
            {
                var logWriter = new SourceLogWriter();

                try
                {
                    postInitializationAction?.Invoke(postContext, logWriter);
                    writingAction.Invoke(logWriter.InnerWriter, logWriter);
                    postContext.AddSource(filename, logWriter.ToString());
                }
                catch (Exception ex)
                {
                    logWriter.Error(ex);
                    postContext.AddSource(filename, logWriter.ToString());
                }
            });
        }

        public AttributeDefinition AddMarkerAttribute(AttributeDefinition definition)
        {
            context.RegisterPostInitializationOutput(
                definition.Filename,
                (writer, log) =>
                {
                    writer.WriteLineWithoutIndentation(definition.ToString());
                },
                (
                    (initializationContext, writer) =>
                    {
                        initializationContext.AddEmbeddedAttributeDefinition();
                    }
                )
            );

            return definition;
        }

        public AttributeDefinition AddMarkerAttribute(
            string name,
            Action<AttributeDefinition> configuration
        )
        {
            var definition = new AttributeDefinition(name);
            configuration.Invoke(definition);

            return context.AddMarkerAttribute(definition);
        }
    }
}
