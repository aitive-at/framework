namespace Aitive.Framework.Functional;

public sealed record OptionalReference<T>(T? Value, bool HasValue)
    where T : class
{
    public static implicit operator Optional<T>(OptionalReference<T> optional)
    {
        return optional.ToOptional();
    }

    public static implicit operator OptionalReference<T>(Optional<T> optional)
    {
        return OptionalReference<T>.FromOptional(optional);
    }

    public Optional<T> ToOptional()
    {
        return HasValue ? Optional.Some(Value!) : Optional.None<T>();
    }

    public static OptionalReference<T> FromOptional(Optional<T> optional)
    {
        return optional
            ? new OptionalReference<T>(optional.Value, true)
            : new OptionalReference<T>(null, false);
    }
}
