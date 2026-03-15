namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// Receives strongly typed callbacks for transfer lifecycle events from a <see cref="DataTransferQueue"/>.
/// Implementations must not throw — an exception from a callback will cause the affected transfer to fail.
/// </summary>
public interface IDataTransferQueueListener
{
    /// <summary>
    /// Called when a transfer is added to the queue but has not yet started
    /// (waiting for a concurrency slot).
    /// </summary>
    void OnTransferEnqueued(DataTransferTask transfer);

    /// <summary>
    /// Called when a transfer acquires a concurrency slot and begins downloading.
    /// </summary>
    void OnTransferStarted(DataTransferTask transfer);

    /// <summary>
    /// Called after each chunk is written. May fire at high frequency.
    /// </summary>
    void OnTransferProgress(DataTransferTask transfer);

    /// <summary>
    /// Called when a transfer finishes successfully (including final hash verification).
    /// </summary>
    void OnTransferCompleted(DataTransferTask transfer);

    /// <summary>
    /// Called when a transfer fails with an exception.
    /// </summary>
    void OnTransferFailed(DataTransferTask transfer);

    /// <summary>
    /// Called when a transfer is cancelled via <see cref="DataTransferQueue.Cancel"/>
    /// or queue disposal.
    /// </summary>
    void OnTransferCancelled(DataTransferTask transfer);
}
