namespace Aitive.Framework.Threading;

public static class CancellationTokenExtensions
{
    extension(CancellationToken cancellationToken)
    {
        public async Task Await(bool throwOnCancellation = false)
        {
            var completionSource = new TaskCompletionSource();

            var token = cancellationToken.Register(() =>
            {
                if (throwOnCancellation)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                }
                else
                {
                    completionSource.TrySetResult();
                }
            });

            try
            {
                await completionSource.Task;
            }
            finally
            {
                await token.DisposeAsync();
            }
        }
    }
}
