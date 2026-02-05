using Aitive.Framework.Versioning.Parsing;

namespace Aitive.Framework.Versioning.Utilities;

internal static class VersionParsing
{
    /// <remarks>
    /// This exception is used with the
    /// <see cref="SemVersionParser.Parse(string?,Aitive.Framework.Versioning.SemVersionStyles,System.Exception?,int,out Aitive.Framework.Versioning.SemVersion?)"/>
    /// method to indicate parse failure without constructing a new exception.
    /// This exception should never be thrown or exposed outside of this
    /// package.
    /// </remarks>
    public static readonly Exception FailedException = new Exception("Parse Failed");
}
