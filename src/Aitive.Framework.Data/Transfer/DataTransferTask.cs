using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// Read-only snapshot of a tracked transfer task.
/// </summary>
public sealed record DataTransferTask(
    Guid Id,
    DataTransferFile File,
    string TargetPath,
    DataTransferState State,
    Optional<DataTransferProgress> Progress = default,
    Optional<Exception> Error = default
);
