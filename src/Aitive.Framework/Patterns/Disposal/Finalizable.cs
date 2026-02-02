namespace Aitive.Framework.Patterns.Disposal;

public abstract class Finalizable : Disposable
{
    protected Finalizable(bool throwOnDoubleDispose = true)
        : base(throwOnDoubleDispose) { }

    protected override void OnBeforeDispose(bool disposing)
    {
        if (disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    ~Finalizable()
    {
        TriggerDispose(false);
    }
}
