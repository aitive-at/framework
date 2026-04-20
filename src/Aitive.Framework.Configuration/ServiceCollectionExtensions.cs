using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.Configuration;

public static class ServiceCollectionExtensions
{
    private static readonly MethodInfo addConfigurationBindingMethod =
        typeof(ServiceCollectionExtensions).GetMethod(
            nameof(AddConfigurationBinding),
            BindingFlags.Static | BindingFlags.NonPublic
        ) ?? throw new NotImplementedException("AddConfigurationBinding not implemented");

    private static void AddConfigurationBinding<T>(IServiceCollection services)
        where T : class
    {
        services.AddSingleton<IConfigureOptions<T>, UntypedConfigurationBinding<T>>();
    }

    extension(IServiceCollection services)
    {
        public void AddConfigurationOptions(Assembly assembly, IConfiguration configuration)
        {
            foreach (var type in assembly.GetTypes().Where(t => t.IsConfigurationOptionsSection))
            {
                services.AddConfigurationOptions(type, configuration);
            }
        }

        public void AddConfigurationOptions(Type optionsType, IConfiguration configuration)
        {
            if (!optionsType.IsConfigurationOptionsSection)
            {
                throw new ArgumentException($"{optionsType} is not a configuration options type");
            }

            services.AddOptions();

            var finalMethod = addConfigurationBindingMethod.MakeGenericMethod(optionsType);
            finalMethod.Invoke(null, [services]);
        }
    }
}
