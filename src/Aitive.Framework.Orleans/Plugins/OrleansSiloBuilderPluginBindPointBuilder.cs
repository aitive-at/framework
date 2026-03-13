using Aitive.Framework.Collections;
using Aitive.Framework.Plugins;
using Aitive.Framework.Plugins.Binding;

namespace Aitive.Framework.Orleans.Plugins;

public sealed class OrleansSiloBuilderPluginBindPointBuilder : IPluginBindPointBuilder
{
    private readonly ISiloBuilder _siloBuilder;
    private readonly IReadOnlyDictionary<Type, object> _initialServices;

    public OrleansSiloBuilderPluginBindPointBuilder(
        ISiloBuilder siloBuilder,
        IReadOnlyDictionary<Type, object> initialServices
    )
    {
        _siloBuilder = siloBuilder;
        _initialServices = initialServices;
    }

    public IPluginBindPoint Build(IReadOnlyList<IPlugin> plugins)
    {
        foreach (
            var configurationStep in plugins.SelectMany(p =>
                p.Query<ISiloModule>(_initialServices).PossiblyOrdered()
            )
        )
        {
            configurationStep.Configure(_siloBuilder);
        }

        return new EmptyPluginBindPoint();
    }
}
