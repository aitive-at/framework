using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy;

public interface ITenantHttpPipeline : ITenant, IAsyncDisposable
{
    Task Invoke(HttpContext context);
}
