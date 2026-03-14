using Aitive.Framework.Cryptography.Hashing.Algorithms;

namespace Aitive.Framework.Data.Transfer;

public record DataTransferFile(string Path, long Size, Sha256Value Hash) { }
