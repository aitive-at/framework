using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.Configuration;

internal sealed class UntypedConfigurationBinding<T>(IConfiguration configuration)
    : IConfigureOptions<T>
    where T : class
{
    public void Configure(T options)
    {
        if (typeof(T).IsConfigurationOptionsSection)
        {
            var sectionName = typeof(T).ConfigurationOptionsSectionName;
            var section = configuration.GetSection(sectionName);

            section.Bind(
                options,
                binderOptions =>
                {
                    binderOptions.ErrorOnUnknownConfiguration = true;
                }
            );
        }
    }
}
