using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy.Routers;

public interface ITenantHttpPathPrefixProvider
{
    ValueTask<Optional<TenantId>> GetTenantIdFromPathPrefix(
        string pathPrefix,
        CancellationToken cancellationToken = default
    );

    ValueTask<Optional<string>> GetPathPrefixFromTenant(
        TenantId tenantId,
        CancellationToken cancellationToken = default
    );
}
