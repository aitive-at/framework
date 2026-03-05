namespace Aitive.Framework.Plugins.Binding;

public sealed class EmptyPluginBindPoint : IPluginBindPoint
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
