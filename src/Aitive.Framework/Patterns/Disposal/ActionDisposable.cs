namespace Aitive.Framework.Patterns.Disposal;

public class ActionDisposable(Action disposalAction, bool throwOnDoubleDispose = true)
    : Disposable(throwOnDoubleDispose)
{
    public static implicit operator ActionDisposable(Action disposalAction)
    {
        return new ActionDisposable(disposalAction);
    }

    protected override void OnDispose(bool disposing)
    {
        disposalAction.Invoke();
    }
}
