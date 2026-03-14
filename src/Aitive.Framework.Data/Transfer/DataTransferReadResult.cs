using Aitive.Framework.Cryptography.Hashing.Algorithms;

namespace Aitive.Framework.Data.Transfer;

public sealed record DataTransferReadResult(long Length, Sha256Value Hash);
