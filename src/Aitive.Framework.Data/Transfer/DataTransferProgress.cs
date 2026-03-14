namespace Aitive.Framework.Data.Transfer;

/// <summary>
/// Progress snapshot reported during a file download.
/// </summary>
public record DataTransferProgress(long BytesTransferred, long TotalBytes, int ChunksCompleted)
{
    public double Fraction => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes : 0.0;
}
