using Aitive.Framework.AspNetCore.Tenancy.Builders;
using Aitive.Framework.AspNetCore.Tenancy.Default;
using Aitive.Framework.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aitive.Framework.AspNetCore.Tenancy;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public ITenancyServiceBuilder AddHttpTenancy(int maxCachedPipelines = 1000)
        {
            services.AddSingleton<ITenantResolver>(sp => new DefaultTenantHttpResolver());
            services.AddSingleton<ITenantHttpPipelineProvider>(
                sp => new DefaultTenantHttpPipelineProvider(
                    sp.GetRequiredService<ITenantHttpRouter>(),
                    sp,
                    maxCachedPipelines
                )
            );

            return new DefaultTenancyServiceBuilder(services);
        }
    }
}
