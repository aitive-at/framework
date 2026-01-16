using Aitive.Framework.Collections;

namespace Aitive.Framework.Functional.Pipelines;

public abstract class MiddlewarePipelineBuilder<T, TStep, TPhase, TSelf>
    : IMiddlewarePipelineBuilder<T, TStep, TPhase, TSelf>
    where TSelf : IMiddlewarePipelineBuilder<T, TStep, TPhase, TSelf>
    where TStep : IMiddleware<T>
    where TPhase : notnull
{
    private readonly IReadOnlyDictionary<TPhase, int> _phaseOrders;
    private readonly Dictionary<TPhase, List<(TStep Step, int Order)>> _phaseBuckets;

    protected MiddlewarePipelineBuilder(IReadOnlyDictionary<TPhase, int> phaseOrders)
    {
        _phaseOrders = new OrderedDictionary<TPhase, int>(phaseOrders);
        _phaseBuckets = _phaseOrders.ToDictionary(
            kv => kv.Key,
            kv => new List<(TStep Step, int Order)>(kv.Value)
        );
    }

    public TSelf AddStep(TPhase phase, TStep step, int order = 0)
    {
        if (!_phaseBuckets.TryGetValue(phase, out var bucket))
        {
            throw new KeyNotFoundException($"The step phase {phase} was not defined");
        }

        bucket.Add((step, order));
        return (TSelf)(object)this;
    }

    public Action<T> Build()
    {
        return _phaseOrders
            .SelectMany(p =>
                _phaseBuckets[p.Key]
                    .Select(s => new
                    {
                        PhaseOrder = p.Value,
                        StepOrder = s.Order,
                        Step = s.Step,
                    })
            )
            .OrderBy(p => p.PhaseOrder)
            .ThenBy(p => p.StepOrder)
            .Select(p => p.Step)
            .Cast<IMiddleware<T>>()
            .Compile();
    }
}
