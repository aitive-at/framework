namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// Configuration for <see cref="DataTransferHandlerExtensions.DownloadToFileAsync"/>.
/// </summary>
public sealed class DataTransferOptions
{
    /// <summary>
    /// Size of each read block in bytes. Defaults to 80 KB.
    /// </summary>
    public int BlockSize { get; init; } = 81_920;

    /// <summary>
    /// When true, deletes any existing file and downloads from the beginning.
    /// </summary>
    public bool ForceRestart { get; init; }

    /// <summary>
    /// Optional progress reporter for tracking download progress.
    /// </summary>
    public IProgress<DataTransferProgress>? Progress { get; init; }
}
