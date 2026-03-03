using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy;

public interface ITenantHttpPipelineProvider
{
    ValueTask<Optional<ITenantHttpPipeline>> GetOrCreate(HttpContext context);

    ValueTask Invalidate(TenantId tenantId);
}
