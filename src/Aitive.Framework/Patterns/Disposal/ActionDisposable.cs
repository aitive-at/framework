namespace Aitive.Framework.Patterns.Disposal;

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
