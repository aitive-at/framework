namespace Aitive.Framework.Functional.Pipelines;

public interface IPipelineBuilder<T, in TStep, TPhase, TSelf>
    where TStep : IPipelineStep<T>
    where TSelf : IPipelineBuilder<T, TStep, TPhase, TSelf>
{
    TSelf AddStep(TPhase phase, TStep step, int order = 0);
}
