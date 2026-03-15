using System.ComponentModel.Design;
using Aitive.Framework.Cryptography.Hashing.Algorithms;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Cryptography.Hashing;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddHashing()
        {
            services.AddSingleton<IHashAlgorithm<Sha256Value>, Sha256ClrHashAlgorithm>();
            services.AddSingleton<IHashAlgorithm<Sha1Value>, Sha1ClrHashAlgorithm>();

            services.AddSingleton<IHashProvider<Sha256Value>, HashProvider<Sha256Value>>();
            services.AddSingleton<IHashProvider<Sha1Value>, HashProvider<Sha1Value>>();
        }
    }
}
