using Aitive.Framework.Collections;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Reflection;

public interface ITypeResolver<TTypeId>
{
    Optional<Type> Resolve(TTypeId id);
    Optional<TTypeId> Resolve(Type type);
}

public sealed class StaticTypeResolver<TTypeId>(IReadOnlyDictionary<TTypeId, Type> idToTypes)
    : ITypeResolver<TTypeId>
    where TTypeId : notnull
{
    private readonly Dictionary<TTypeId, Type> _idToTypes = new(idToTypes);
    private readonly Dictionary<Type, TTypeId> _typeToId = idToTypes.ToDictionary(
        k => k.Value,
        v => v.Key
    );

    public Optional<Type> Resolve(TTypeId id)
    {
        return _idToTypes.GetOptional(id);
    }

    public Optional<TTypeId> Resolve(Type type)
    {
        return _typeToId.GetOptional(type);
    }
}

public sealed class CombinedTypeResolver<TTypeId>(IEnumerable<ITypeResolver<TTypeId>> resolvers)
    : ITypeResolver<TTypeId>
{
    private readonly IReadOnlyList<ITypeResolver<TTypeId>> _resolvers = resolvers.ToList();

    public Optional<Type> Resolve(TTypeId id)
    {
        return _resolvers.Select(r => r.Resolve(id)).FirstOrNone();
    }

    public Optional<TTypeId> Resolve(Type type)
    {
        return _resolvers.Select(r => r.Resolve(type)).FirstOrNone();
    }
}
