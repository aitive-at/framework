namespace Aitive.Framework.Functional.Errors;

public interface IError { }

public interface IError<T> : IError
    where T : IError<T>
{
    static IErrorBuilder<T> Error(string id)
    {
        throw new NotImplementedException();
    }
}

public interface IMyError : IError<IMyError>
{
    static IMyError M1 => Error("").Build();
}

public interface IBaseError<T> : IError<T>
    where T : IError<T>
{
    T E1 => Error("").Build();
}
