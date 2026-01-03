namespace Aitive.Framework.Functional.Pipelines;

public interface IPipelineStep<in T>
{
    void Invoke(T input,Action next);
}

public static class PipelineStepExtensions
{
    extension<T>(IEnumerable<IPipelineStep<T>> steps)
    {
        public Action<T> Compile()
        {
            Action<T> next = i => { };
            
            foreach (var step in steps.Reverse())
            {
                var localNext = next;
                next = i => step.Invoke(i,() => localNext(i));
            }

            return next;
        }
    }
}