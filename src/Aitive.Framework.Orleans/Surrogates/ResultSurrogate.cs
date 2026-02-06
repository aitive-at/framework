using Aitive.Framework.Functional;

namespace Aitive.Framework.Orleans.Surrogates;

[GenerateSerializer]
public struct ResultSurrogate<T, TError>
{
    [Id(0)]
    public T Value { get; set; }

    [Id(1)]
    public TError Error { get; set; }

    [Id(2)]
    public bool WasSuccessful { get; set; }
}

[RegisterConverter]
public sealed class ResultSurrogateConverter<T, TError>
    : IConverter<Result<T, TError>, ResultSurrogate<T, TError>>
{
    public Result<T, TError> ConvertFromSurrogate(in ResultSurrogate<T, TError> surrogate)
    {
        return surrogate.WasSuccessful
            ? new Result<T, TError>(surrogate.Value!)
            : new Result<T, TError>(surrogate.Error);
    }

    public ResultSurrogate<T, TError> ConvertToSurrogate(in Result<T, TError> value)
    {
        return value
            ? new ResultSurrogate<T, TError>()
            {
                Value = value.Value,
                WasSuccessful = value.WasSuccessful,
            }
            : new ResultSurrogate<T, TError>()
            {
                Error = value.Error,
                WasSuccessful = value.WasSuccessful,
            };
    }
}
