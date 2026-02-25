namespace Aitive.Framework.Functional.Errors;

public interface IErrorBuilder<T>
    where T : IError<T>
{
    public T Build()
    {
        throw new NotImplementedException();
    }
}
