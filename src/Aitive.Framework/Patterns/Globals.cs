using System.Collections.Concurrent;

namespace Aitive.Framework.Patterns;

public static class Globals
{
    private static readonly ConcurrentDictionary<Type, object> _entries;
    private static readonly ConcurrentQueue<Func<Type, object?>> _resolvers;
}
