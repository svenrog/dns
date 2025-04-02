namespace DNS.Benchmark.Baseline.Protocol.Utils;

public static class BaselineTaskExtensions
{
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token)
    {
        TaskCompletionSource<bool> tcs = new();
        CancellationTokenRegistration registration = token.Register(src =>
        {
            ((TaskCompletionSource<bool>)src).TrySetResult(true);
        }, tcs);

        using (registration)
            if (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false) != task)
                throw new OperationCanceledException(token);

        return await task.ConfigureAwait(false);
    }

    public static async Task<T> WithCancellationTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource linkSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);

        return await task.WithCancellation(linkSource.Token).ConfigureAwait(false);
    }
}
