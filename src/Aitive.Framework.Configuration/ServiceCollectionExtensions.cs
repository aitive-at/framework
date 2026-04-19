using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Configuration;

public static class ServiceCollectionExtensions
{
    private static readonly MethodInfo _configureMethod =
        typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethods()
            .First(m =>
                m is { Name: "Configure", IsGenericMethodDefinition: true }
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(IConfiguration)
            );

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

            var sectionName = optionsType.ConfigurationOptionsSectionName;

            // Make it generic for your options type
            var generic = _configureMethod.MakeGenericMethod(optionsType);

            // Invoke: services.Configure<YourType>(configuration.GetSection(sectionName));
            generic.Invoke(
                obj: null,
                parameters: [services, configuration.GetSection(sectionName)]
            );
        }
    }
}
