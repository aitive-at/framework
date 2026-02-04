using System.Collections.Concurrent;

namespace Aitive.Framework.Patterns;

public static class Globals
{
    private static readonly ConcurrentDictionary<Type, object> _entries = new();
    private static volatile IServiceProvider? _serviceProvider;

    public static void Register<T>(T service)
        where T : notnull
    {
        Register(service, typeof(T));
    }

    public static void Register(object service, Type? type = null)
    {
        var registryType = type ?? service.GetType();

        if (registryType == typeof(IServiceProvider))
        {
            _serviceProvider = (IServiceProvider)service;
        }

        _entries[registryType] = service;
    }

    public static T Resolve<T>()
        where T : notnull
    {
        return (T)Resolve(typeof(T));
    }

    public static object Resolve(Type type)
    {
        if (_entries.TryGetValue(type, out var result))
        {
            return result;
        }

        var serviceProvider = _serviceProvider;

        if (serviceProvider != null)
        {
            result = serviceProvider.GetService(type);

            if (result != null)
            {
                return result;
            }
        }

        throw new InvalidOperationException($"Global not registered:  {type}");
    }
}
