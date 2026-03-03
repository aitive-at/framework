using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ForwardSingleton<TSource, TTarget>()
            where TSource : class
            where TTarget : class
        {
            services.AddSingleton<TTarget>(sp => (TTarget)(object)sp.GetRequiredService<TSource>());
            return services;
        }

        public IServiceCollection ForwardSingleton(Type sourceType, Type targetType)
        {
            services.AddSingleton(targetType, sp => sp.GetRequiredService(sourceType));
            return services;
        }
    }
}
