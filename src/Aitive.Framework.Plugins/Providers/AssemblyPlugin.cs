using System.Reflection;
using Aitive.Framework.Collections;
using Aitive.Framework.Functional;
using Aitive.Framework.Patterns.Disposal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Aitive.Framework.Plugins.Providers;

public abstract class AssemblyPlugin : IPlugin, IAsyncDisposable
{
    private readonly HashSet<Type> _types;
    private readonly Dictionary<Type, object> _instances;

    protected AssemblyPlugin(PluginManifest manifest, Assembly assembly)
    {
        Manifest = manifest;

        _types = assembly.GetTypes().Where(IsValidPluginType).ToHashSet();
        _instances = new Dictionary<Type, object>();
        Assembly = assembly;
    }

    public PluginManifest Manifest { get; }

    protected Assembly Assembly { get; }

    public IEnumerable<object> Query(Type interfaceType, IReadOnlyDictionary<Type, object> services)
    {
        return QueryCore(interfaceType, services);
    }

    protected virtual IEnumerable<object> QueryCore(
        Type interfaceType,
        IReadOnlyDictionary<Type, object> services
    )
    {
        if (interfaceType == typeof(Assembly))
        {
            return [Assembly];
        }

        var result = new List<object>();

        foreach (var type in _types)
        {
            if (type.IsAssignableTo(interfaceType))
            {
                var localType = type;

                // Check if we have an instance
                var instance = _instances
                    .GetOptional(type)
                    .Or(() => CreateInstance(localType, services).Value)!;

                if (_instances.ContainsKey(localType))
                {
                    _instances[localType] = instance;
                }

                result.Add(instance);
            }
        }

        return result;
    }

    protected virtual Optional<object> CreateInstance(
        Type type,
        IReadOnlyDictionary<Type, object> services
    )
    {
        var constructor = FindBestConstructor(type, services);

        if (constructor)
        {
            var instance = Activator.CreateInstance(type, [.. constructor.Value]);

            if (instance != null)
            {
                return Optional.Some(instance);
            }
        }

        return Optional.None<object>();
    }

    protected virtual bool IsValidPluginType(Type type)
    {
        return type is { IsClass: true, IsPublic: true, IsAbstract: false }
            && type.GetConstructors().Any(ctor => ctor.IsPublic);
    }

    protected Optional<IReadOnlyList<object>> FindBestConstructor(
        Type type,
        IReadOnlyDictionary<Type, object> services
    )
    {
        Optional<IReadOnlyList<object>> result = Optional.None<IReadOnlyList<object>>();

        foreach (
            var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
        )
        {
            var parameters = constructor.GetParameters();

            if (!result || parameters.Length > result.Value.Count)
            {
                var resultCollection = new List<object>(parameters.Length);

                foreach (var parameter in parameters)
                {
                    if (services.TryGetValue(parameter.ParameterType, out var value))
                    {
                        resultCollection.Add(value);
                    }
                    else
                    {
                        break;
                    }
                }

                if (resultCollection.Count == parameters.Length)
                {
                    // Valid
                    result = Optional.Some<IReadOnlyList<object>>(resultCollection);
                }
            }
        }

        return result;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var instance in _instances.Values)
        {
            await instance.EnsureDisposal();
        }
    }
}
