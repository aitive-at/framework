using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Logging;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework;

public abstract class IncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootLogger = new SourceLogWriter();

        try
        {
            OnInitialize(context, rootLogger);
        }
        catch (Exception ex)
        {
            rootLogger.Error(ex);
        }
        finally
        {
            if (rootLogger.ErrorCount > 0)
            {
                context.RegisterPostInitializationOutput(post =>
                {
                    post.AddSource("__InitializationErrors.g.cs", rootLogger.ToString());
                });
            }
        }
    }

    protected abstract void OnInitialize(
        IncrementalGeneratorInitializationContext context,
        ILogWriter logWriter
    );
}
