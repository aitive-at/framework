using System.Runtime.ExceptionServices;
using Aitive.Framework.Diagnostics.Exceptions;
using Aitive.Framework.Functional.Pipelines;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.Application.Hosting;

public abstract class Application<TBuilder, THost>
    where TBuilder : IHostApplicationBuilder
    where THost : IHost
{
    protected Application(ApplicationOptions options, IApplicationDescription description)
    {
        Options = options;
        Description = description;
    }

    public ApplicationOptions Options { get; }

    public IApplicationDescription Description { get; }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        var exceptionHandler = OnGetExceptionHandlers().Compile();

        try
        {
            var builder = OnCreateBuilder();
            OnConfigureBuilder(builder);

            using var host = OnBuildHost(builder);
            OnConfigureHost(host);

            await host.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var dispatchInfo = ExceptionDispatchInfo.Capture(ex);
            var exceptionContext = new ExceptionHandlerContext(dispatchInfo);
            exceptionHandler(exceptionContext);
            exceptionContext.RethrowUnhandled();
        }
    }

    protected abstract TBuilder OnCreateBuilder();

    protected abstract void OnConfigureBuilder(TBuilder builder);

    protected abstract void OnConfigureHost(THost host);

    protected abstract THost OnBuildHost(TBuilder builder);

    protected virtual IEnumerable<IExceptionHandler> OnGetExceptionHandlers()
    {
        yield break;
    }
}
