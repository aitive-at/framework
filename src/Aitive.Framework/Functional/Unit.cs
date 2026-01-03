using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Aitive.Framework.Functional;

public readonly struct Unit : 
    IEquatable<Unit>,
    IComparable<Unit>,
    IComparisonOperators<Unit, Unit, bool>
{
    public static readonly Unit Default = default;
    
    public override bool Equals(
        [NotNullWhen(true)]
        object? obj)
    {
        return obj != null && obj.GetType() == this.GetType();
    }

    public override int GetHashCode()
    {
        return 42;
    }

    public bool Equals(Unit other)
    {
        return true;
    }

    public int CompareTo(Unit other)
    {
        return 0;
    }

    public static bool operator ==(Unit left,
        Unit right)
    {
        return true;
    }

    public static bool operator !=(Unit left,
        Unit right)
    {
        return false;
    }

    public static bool operator >(Unit left,
        Unit right)
    {
        return false;
    }

    public static bool operator >=(Unit left,
        Unit right)
    {
        return true;
    }

    public static bool operator <(Unit left,
        Unit right)
    {
        return false;
    }

    public static bool operator <=(Unit left,
        Unit right)
    {
        return true;
    }
}