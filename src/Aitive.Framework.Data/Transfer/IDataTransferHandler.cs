using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Transfer;

public interface IDataTransferHandler
{
    Task<Result<DataTransferReadResult, DataTransferError>> Read(
        Memory<byte> target,
        long offset,
        CancellationToken cancellationToken = default
    );
}
