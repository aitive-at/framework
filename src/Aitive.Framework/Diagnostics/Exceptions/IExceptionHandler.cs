using Aitive.Framework.Functional.Pipelines;

namespace Aitive.Framework.Diagnostics.Exceptions;

public interface IExceptionHandler : IMiddleware<ExceptionHandlerContext> { }
