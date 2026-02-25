using System.Runtime.CompilerServices;

namespace Aitive.Framework.Functional.Errors;

public sealed class ErrorRegistry
{
    public static IErrorBuilder<T> GetError<T>(string id)
        where T : IError<T>
    {
        throw new NotImplementedException();
    }
}
