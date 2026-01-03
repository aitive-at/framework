namespace Aitive.Framework.Functional.Pipelines;

public interface IAsyncPipelineStep<in T>
{
    Task Invoke(T input,Func<Task> next);
}


public static class AsyncPipelineStepExtensions
{
    extension<T>(IEnumerable<IAsyncPipelineStep<T>> steps)
    {
        public Func<T,Task> Compile()
        {
            Func<T,Task> next = i => Task.CompletedTask;
            
            foreach (var step in steps.Reverse())
            {
                var localNext = next;
                next = i => step.Invoke(i,() => localNext(i));
            }

            return next;
        }
    }
}