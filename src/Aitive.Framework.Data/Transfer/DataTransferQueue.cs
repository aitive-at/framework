using Aitive.Framework.Cryptography.Hashing;
using Aitive.Framework.Cryptography.Hashing.Algorithms;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// A thread-safe, disposable queue that runs data transfers in the background
/// with bounded concurrency. All transfers use the <see cref="IDataTransferHandler"/>
/// supplied at construction time.
/// </summary>
public sealed class DataTransferQueue : IAsyncDisposable
{
    private readonly IDataTransferHandler _handler;
    private readonly IDataTransferQueueListener _listener;
    private readonly IHashProvider<Sha256Value> _hashProvider;
    private readonly SemaphoreSlim _semaphore;
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, TrackedTransfer> _transfers = [];
    private readonly CancellationTokenSource _disposalCts = new();

    private volatile bool _disposed;

    public DataTransferQueue(
        IDataTransferHandler handler,
        IDataTransferQueueListener listener,
        IHashProvider<Sha256Value> hashProvider,
        int maxConcurrentTransfers = 4
    )
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(listener);
        ArgumentNullException.ThrowIfNull(hashProvider);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrentTransfers, 1);

        _handler = handler;
        _listener = listener;
        _hashProvider = hashProvider;
        _semaphore = new SemaphoreSlim(maxConcurrentTransfers, maxConcurrentTransfers);
    }

    /// <summary>
    /// Enqueues a transfer. It will start once a concurrency slot is available.
    /// </summary>
    public Guid Add(
        DataTransferFile file,
        string targetPath,
        Optional<DataTransferOptions> options = default
    )
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var id = Guid.NewGuid();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_disposalCts.Token);
        var resolvedOptions = options.Or(new DataTransferOptions());

        var tracked = new TrackedTransfer
        {
            Id = id,
            File = file,
            TargetPath = targetPath,
            Options = resolvedOptions,
            CancellationTokenSource = cts,
        };

        lock (_lock)
        {
            _transfers[id] = tracked;
        }

        _listener.OnTransferEnqueued(tracked.ToSnapshot());

        tracked.RunningTask = Task.Run(() => ExecuteTransferAsync(tracked), cts.Token);

        return id;
    }

    /// <summary>
    /// Cancels a pending or running transfer. Returns false if already terminal or not found.
    /// </summary>
    public bool Cancel(Guid transferId)
    {
        TrackedTransfer? tracked;
        lock (_lock)
        {
            if (!_transfers.TryGetValue(transferId, out tracked))
            {
                return false;
            }
        }

        if (
            tracked.State
            is DataTransferState.Completed
                or DataTransferState.Failed
                or DataTransferState.Cancelled
        )
        {
            return false;
        }

        tracked.CancellationTokenSource.Cancel();
        return true;
    }

    /// <summary>
    /// Removes a transfer from tracking. Cancels it first if still active.
    /// Returns false if not found.
    /// </summary>
    public bool Remove(Guid transferId)
    {
        TrackedTransfer? tracked;
        lock (_lock)
        {
            if (!_transfers.Remove(transferId, out tracked))
            {
                return false;
            }
        }

        tracked.CancellationTokenSource.Cancel();
        tracked.CancellationTokenSource.Dispose();
        return true;
    }

    /// <summary>
    /// Returns a snapshot of the specified transfer.
    /// </summary>
    public Optional<DataTransferTask> Get(Guid transferId)
    {
        lock (_lock)
        {
            return _transfers.TryGetValue(transferId, out var tracked)
                ? Optional.Some(tracked.ToSnapshot())
                : Optional<DataTransferTask>.None;
        }
    }

    /// <summary>
    /// Returns snapshots of all tracked transfers.
    /// </summary>
    public IReadOnlyList<DataTransferTask> GetAll()
    {
        lock (_lock)
        {
            return _transfers.Values.Select(t => t.ToSnapshot()).ToList();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _disposalCts.CancelAsync();

        Task[] pending;
        lock (_lock)
        {
            pending = _transfers
                .Values.Select(t => t.RunningTask)
                .Where(t => t is not null)
                .ToArray()!;
        }

        if (pending.Length > 0)
        {
            try
            {
                await Task.WhenAll(pending);
            }
            catch
            {
                // Expected — cancelled/failed tasks during disposal.
            }
        }

        lock (_lock)
        {
            foreach (var tracked in _transfers.Values)
            {
                tracked.CancellationTokenSource.Dispose();
            }

            _transfers.Clear();
        }

        _semaphore.Dispose();
        _disposalCts.Dispose();
    }

    // ──────────────────────────────────────────────────────────────────
    //  Transfer execution
    // ──────────────────────────────────────────────────────────────────

    private async Task ExecuteTransferAsync(TrackedTransfer tracked)
    {
        var acquired = false;
        try
        {
            await _semaphore.WaitAsync(tracked.CancellationTokenSource.Token);
            acquired = true;

            tracked.State = DataTransferState.Running;
            _listener.OnTransferStarted(tracked.ToSnapshot());

            var transferOptions = new DataTransferOptions
            {
                BlockSize = tracked.Options.BlockSize,
                ForceRestart = tracked.Options.ForceRestart,
                Progress = new Progress<DataTransferProgress>(p =>
                {
                    tracked.Progress = p;
                    tracked.Options.Progress?.Report(p);
                    _listener.OnTransferProgress(tracked.ToSnapshot());
                }),
            };

            await _handler.TransferToFileAsync(
                _hashProvider,
                tracked.File,
                tracked.TargetPath,
                transferOptions,
                tracked.CancellationTokenSource.Token
            );

            tracked.State = DataTransferState.Completed;
            _listener.OnTransferCompleted(tracked.ToSnapshot());
        }
        catch (OperationCanceledException)
        {
            tracked.State = DataTransferState.Cancelled;
            _listener.OnTransferCancelled(tracked.ToSnapshot());
        }
        catch (Exception ex)
        {
            tracked.State = DataTransferState.Failed;
            tracked.Error = ex;
            _listener.OnTransferFailed(tracked.ToSnapshot());
        }
        finally
        {
            if (acquired)
            {
                _semaphore.Release();
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Internal mutable state holder
    // ──────────────────────────────────────────────────────────────────

    private sealed class TrackedTransfer
    {
        public required Guid Id { get; init; }
        public required DataTransferFile File { get; init; }
        public required string TargetPath { get; init; }
        public required DataTransferOptions Options { get; init; }
        public required CancellationTokenSource CancellationTokenSource { get; init; }

        public volatile DataTransferState State;
        public DataTransferProgress? Progress;
        public Exception? Error;
        public Task? RunningTask;

        public DataTransferTask ToSnapshot() =>
            new(
                Id,
                File,
                TargetPath,
                State,
                Progress.NullableAsOptional(),
                Error.NullableAsOptional()
            );
    }
}
