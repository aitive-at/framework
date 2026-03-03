namespace Aitive.Framework.Patterns.Disposal;

public sealed class IdemPotentDisposable : IDisposable, IAsyncDisposable
{
    public static readonly IdemPotentDisposable Instance = new();

    public void Dispose() { }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
