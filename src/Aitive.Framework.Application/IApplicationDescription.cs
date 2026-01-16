using Semver;

namespace Aitive.Framework.Application;

public interface IApplicationDescription
{
    ApplicationId Id { get; }

    string Name { get; }

    string Description { get; }

    SemVersion Version { get; }

    string Copyright { get; }
}
