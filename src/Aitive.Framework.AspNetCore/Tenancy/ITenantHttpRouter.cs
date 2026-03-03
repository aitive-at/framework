using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy;

public interface ITenantHttpRouter
{
    ValueTask<Optional<TenantId>> Route(HttpContext context);

    ValueTask<IDisposable> Rewrite(HttpContext context, TenantId tenantId);
}
