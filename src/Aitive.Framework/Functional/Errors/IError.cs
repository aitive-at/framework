namespace Aitive.Framework.Functional.Errors;

public interface IError { }

public interface IError<T> : IError
    where T : IError
{
    static T GetError(string id)
    {
        throw new NotImplementedException();
    }
}

public partial interface IMyError : IError<IMyError>
{
    static partial IMyError M1 { get; }
}

public partial interface IMyError
{
    static partial IMyError M1 => GetError("");
}
