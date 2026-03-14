using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Aitive.Framework.Functional;

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
        return TryResolve(type, out object? result)
            ? result
            : throw new InvalidOperationException($"Global not registered:  {type}");
    }

    public static T ResolveValueOrRegistered<T>(T? value)
        where T : notnull
    {
        return value ?? Resolve<T>();
    }

    public static T ResolveValueOrRegistered<T>(Optional<T> value)
        where T : notnull
    {
        return value ? value.Value : Resolve<T>();
    }

    public static T ResolveRegisteredOrDefault<T>(T value)
        where T : notnull
    {
        if (TryResolve(typeof(T), out object? result))
        {
            return (T)result;
        }

        return value;
    }

    public static bool TryResolve(Type type, [NotNullWhen(true)] out object? result)
    {
        if (_entries.TryGetValue(type, out result))
        {
            return true;
        }

        var serviceProvider = _serviceProvider;

        if (serviceProvider != null)
        {
            result = serviceProvider.GetService(type);
            return result != null;
        }

        return false;
    }
}
