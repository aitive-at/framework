namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// Lifecycle state of a transfer task.
/// </summary>
public enum DataTransferState
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}
