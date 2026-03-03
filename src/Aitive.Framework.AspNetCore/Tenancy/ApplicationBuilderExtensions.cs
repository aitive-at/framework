using Microsoft.AspNetCore.Builder;

namespace Aitive.Framework.AspNetCore.Tenancy;

public static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder applicationBuilder)
    {
        public IApplicationBuilder UseHttpTenancy()
        {
            applicationBuilder.UseMiddleware<TenantHttpMiddleware>();
            return applicationBuilder;
        }
    }
}
