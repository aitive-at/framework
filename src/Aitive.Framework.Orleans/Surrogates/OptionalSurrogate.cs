using Aitive.Framework.Functional;

namespace Aitive.Framework.Orleans.Surrogates;

[GenerateSerializer]
public struct OptionalSurrogate<T>
{
    [Id(0)]
    public T? Value { get; set; }

    [Id(1)]
    public bool HasValue { get; set; }
}

[RegisterConverter]
public sealed class OptionalSurrogateConverter<T> : IConverter<Optional<T>, OptionalSurrogate<T>>
{
    public Optional<T> ConvertFromSurrogate(in OptionalSurrogate<T> surrogate)
    {
        return surrogate.HasValue ? Optional.Some(surrogate.Value!) : Optional.None<T>();
    }

    public OptionalSurrogate<T> ConvertToSurrogate(in Optional<T> value)
    {
        return value
            ? new OptionalSurrogate<T>() { HasValue = true, Value = value.Value }
            : new OptionalSurrogate<T>() { HasValue = false };
    }
}
