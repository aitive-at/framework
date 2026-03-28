using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.Application.Lifetime;

public static class HostApplicationLifetimeExtensions
{
    extension(IHostApplicationLifetime lifetime)
    {
        public Task AwaitStarted(CancellationToken cancellationToken = default)
        {
            return Await(
                lifetime,
                (
                    (applicationLifetime, action) =>
                        applicationLifetime.ApplicationStarted.Register(action)
                ),
                cancellationToken
            );
        }

        public Task AwaitStopping(CancellationToken cancellationToken = default)
        {
            return Await(
                lifetime,
                (
                    (applicationLifetime, action) =>
                        applicationLifetime.ApplicationStopping.Register(action)
                ),
                cancellationToken
            );
        }

        public Task AwaitStopped(CancellationToken cancellationToken = default)
        {
            return Await(
                lifetime,
                (
                    (applicationLifetime, action) =>
                        applicationLifetime.ApplicationStopped.Register(action)
                ),
                cancellationToken
            );
        }
    }

    private static async Task Await(
        IHostApplicationLifetime lifetime,
        Func<IHostApplicationLifetime, Action, IAsyncDisposable> subscriber,
        CancellationToken cancellationToken
    )
    {
        var completionSource = new TaskCompletionSource();
        var cancelRegistration = cancellationToken.Register(() =>
            completionSource.TrySetCanceled(cancellationToken)
        );
        var startRegistration = subscriber.Invoke(lifetime, () => completionSource.TrySetResult());

        try
        {
            await completionSource.Task;
        }
        finally
        {
            await cancelRegistration.DisposeAsync();
            await startRegistration.DisposeAsync();
        }
    }
}
