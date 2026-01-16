using Aitive.Framework.Cryptography.Hashing.Algorithms;
using Aitive.Framework.GeneratedCode;

namespace Aitive.Framework.Vcs.Trees;

[TypedId]
public readonly partial record struct TreeId(Sha256Value Value) { }
