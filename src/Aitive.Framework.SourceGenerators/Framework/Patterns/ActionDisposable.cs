namespace Aitive.Framework.SourceGenerators.Framework.Patterns;

public class ActionDisposable(Action disposalAction) : Disposable
{
    public static implicit operator ActionDisposable(Action disposalAction)
    {
        return new ActionDisposable(disposalAction);
    }

    protected override void OnDispose()
    {
        disposalAction.Invoke();
    }
}
