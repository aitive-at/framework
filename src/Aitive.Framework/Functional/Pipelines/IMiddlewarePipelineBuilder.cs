namespace Aitive.Framework.Functional.Pipelines;

public interface IMiddlewarePipelineBuilder<T, in TStep, in TPhase, out TSelf>
    where TStep : IMiddleware<T>
    where TSelf : IMiddlewarePipelineBuilder<T, TStep, TPhase, TSelf>
    where TPhase : notnull
{
    TSelf AddStep(TPhase phase, TStep step, int order = 0);

    Action<T> Build();
}
