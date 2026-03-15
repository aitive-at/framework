using Aitive.Framework.Cryptography.Hashing.Algorithms;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Transfer;

public sealed record DataTransferReadResult(long Length, Optional<Sha256Value> Hash);
