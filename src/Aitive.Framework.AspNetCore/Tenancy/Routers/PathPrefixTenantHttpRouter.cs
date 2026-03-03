using Aitive.Framework.Functional;
using Aitive.Framework.Patterns.Disposal;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy.Routers;

internal sealed class PathPrefixTenantHttpRouter : ITenantHttpRouter
{
    private readonly ITenantHttpPathPrefixProvider _pathPrefixProvider;

    internal PathPrefixTenantHttpRouter(ITenantHttpPathPrefixProvider pathPrefixProvider)
    {
        _pathPrefixProvider = pathPrefixProvider;
    }

    public async ValueTask<Optional<TenantId>> Route(HttpContext context)
    {
        var pathPrefix = GetFirstPathSegment(context.Request.Path);

        if (pathPrefix)
        {
            return await _pathPrefixProvider.GetTenantIdFromPathPrefix(
                pathPrefix.Value,
                context.RequestAborted
            );
        }

        return Optional.None<TenantId>();
    }

    public async ValueTask<IDisposable> Rewrite(HttpContext context, TenantId tenantId)
    {
        var tenantPrefix = await _pathPrefixProvider.GetPathPrefixFromTenant(
            tenantId,
            context.RequestAborted
        );

        if (tenantPrefix)
        {
            PathString prefix = "/" + tenantPrefix.Value;

            var oldPathBase = context.Request.PathBase;
            var oldPath = context.Request.Path;

            context.Request.PathBase += prefix;
            context.Request.Path.StartsWithSegments(
                prefix,
                StringComparison.OrdinalIgnoreCase,
                out var remainingPath
            );
            context.Request.Path = remainingPath;

            return new ActionDisposable(() =>
            {
                context.Request.PathBase = oldPathBase;
                context.Request.Path = oldPath;
            });
        }

        return IdemPotentDisposable.Instance;
    }

    private Optional<string> GetFirstPathSegment(PathString path)
    {
        var value = path.Value ?? string.Empty; // e.g. "/foo/bar/baz"

        var firstSegment = Optional.None<string>();

        if (!string.IsNullOrEmpty(value))
        {
            // Trim leading slash, then take up to next slash
            var trimmed = value.TrimStart('/');
            var slashIndex = trimmed.IndexOf('/');

            firstSegment = slashIndex >= 0 ? trimmed.Substring(0, slashIndex) : trimmed; // only one segment
        }

        return firstSegment;
    }
}
