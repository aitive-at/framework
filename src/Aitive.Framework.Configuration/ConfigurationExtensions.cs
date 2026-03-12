using Microsoft.Extensions.Configuration;

namespace Aitive.Framework.Configuration;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public string GetRequiredConnectionString(string key)
        {
            var connectionString = configuration.GetConnectionString(key);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Connection string {key} not found");
            }

            return connectionString;
        }
    }
}
