using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Aitive.Framework.AspNetCore.Tenancy;

public interface ITenantHttpModuleProvider
{
    Task<IReadOnlyList<IHttpModule>> GetHttpModules(
        TenantId tenant,
        CancellationToken cancellationToken = default
    );
}
