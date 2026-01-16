using Aitive.Framework.Functional.Pipelines;

namespace Aitive.Framework.Configuration;

public enum ConfigurationPhase { }

public interface IConfigurationPipelineBuilder
    : IMiddlewarePipelineBuilder<
        ConfigurationContext,
        IConfigurationStep,
        ConfigurationPhase,
        IConfigurationPipelineBuilder
    > { }
