namespace Aitive.Framework.Functional.Pipelines;

public interface IMiddleware<in T>
{
    void Invoke(T input, Action next);
}

public static class MiddlewareExtensions
{
    extension<T>(IEnumerable<IMiddleware<T>> steps)
    {
        public Action<T> Compile()
        {
            Action<T> next = i => { };

            foreach (var step in steps.Reverse())
            {
                var localNext = next;
                next = i => step.Invoke(i, () => localNext(i));
            }

            return next;
        }
    }
}
