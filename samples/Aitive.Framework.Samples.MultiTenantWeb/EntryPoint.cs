using Aitive.Framework.AspNetCore;
using Aitive.Framework.AspNetCore.Tenancy;
using Aitive.Framework.AspNetCore.Tenancy.Routers;
using Aitive.Framework.Collections;
using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddHttpTenancy()
    .WithPathPrefixRouting()
    .WithPathPrefixProvider<TenantHttpPathPrefixProvider>();

builder.Services.AddSingleton<ITenantHttpModuleProvider, TenantHttpModuleProvider>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseHttpTenancy();

app.MapGet("/", () => Results.Ok("Root"));

app.MapGet("c1/c2", () => Results.Ok("c1/c2"));

app.Run();

class TenantHttpModuleProvider : ITenantHttpModuleProvider
{
    public async Task<IReadOnlyList<IHttpModule>> GetHttpModules(
        TenantId tenant,
        CancellationToken cancellationToken = default
    )
    {
        if (tenant == "tenant1")
        {
            return [new SharedHttpModule(), new Tenant1HttpModule()];
        }

        if (tenant == "tenant2")
        {
            return [new SharedHttpModule(), new Tenant2HttpModule()];
        }

        return [];
    }
}

class SharedHttpModule : IHttpModule
{
    public void Register(
        IServiceProvider services,
        IApplicationBuilder app,
        IEndpointRouteBuilder routes
    )
    {
        routes.MapGet("/s", () => Results.Ok("Shared"));
    }
}

class Tenant1HttpModule : IHttpModule
{
    public void Register(
        IServiceProvider services,
        IApplicationBuilder app,
        IEndpointRouteBuilder routes
    )
    {
        routes.MapGet("/t", () => Results.Ok("tenant1"));
    }
}

class Tenant2HttpModule : IHttpModule
{
    public void Register(
        IServiceProvider services,
        IApplicationBuilder app,
        IEndpointRouteBuilder routes
    )
    {
        routes.MapGet("/t", () => Results.Ok("tenant2"));
    }
}

class TenantHttpPathPrefixProvider : ITenantHttpPathPrefixProvider
{
    private static readonly Dictionary<TenantId, string> _prefixes = new()
    {
        { "tenant1", "a" },
        { "tenant2", "b" },
    };

    public ValueTask<Optional<TenantId>> GetTenantIdFromPathPrefix(
        string pathPrefix,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var prefix in _prefixes)
        {
            if (prefix.Value.Equals(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult(Optional.Some(prefix.Key));
            }
        }

        return ValueTask.FromResult(Optional.None<TenantId>());
    }

    public ValueTask<Optional<string>> GetPathPrefixFromTenant(
        TenantId tenantId,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult(_prefixes.GetOptional(tenantId));
    }
}
