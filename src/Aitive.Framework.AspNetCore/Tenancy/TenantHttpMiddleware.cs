using Aitive.Framework.AspNetCore.Tenancy.Default;
using Aitive.Framework.Functional;
using Aitive.Framework.Patterns.Disposal;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Tenancy;

public sealed class TenantHttpMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantHttpPipelineProvider _tenantHttpPipelineProvider;

    public TenantHttpMiddleware(
        RequestDelegate next,
        ITenantHttpPipelineProvider tenantHttpPipelineProvider
    )
    {
        _next = next;
        _tenantHttpPipelineProvider = tenantHttpPipelineProvider;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var tenantPipeline = await _tenantHttpPipelineProvider.GetOrCreate(httpContext);

        if (tenantPipeline)
        {
            await using (tenantPipeline.Value)
            {
                using (ActivateTenantPipeline(tenantPipeline.Value, httpContext))
                {
                    await tenantPipeline.Value.Invoke(httpContext);
                }
            }
        }
        else
        {
            await _next(httpContext);
        }
    }

    private IDisposable ActivateTenantPipeline(
        ITenantHttpPipeline tenantPipeline,
        HttpContext httpContext
    )
    {
        var oldServices = httpContext.RequestServices;
        httpContext.Tenant = Optional.Some<ITenant>(tenantPipeline);
        httpContext.RequestServices = tenantPipeline.Services;

        var tenantRemovers = new MultiDisposable(
            httpContext
                .RequestServices.GetServices<ITenantResolver>()
                .OfType<DefaultTenantHttpResolver>()
                .ToList()
                .Select(t => t.SetTenantPipeline(tenantPipeline))
        );

        return new ActionDisposable(() =>
        {
            tenantRemovers.Dispose();
            httpContext.RequestServices = oldServices;
            httpContext.Tenant = Optional.None<ITenant>();
        });
    }
}
