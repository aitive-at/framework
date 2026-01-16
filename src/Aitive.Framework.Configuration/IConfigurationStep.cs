using Aitive.Framework.Functional.Pipelines;

namespace Aitive.Framework.Configuration;

public interface IConfigurationStep : IMiddleware<ConfigurationContext> { }
