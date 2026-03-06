namespace Aitive.Framework.Plugins.Resolution;

/// <summary>
/// Controls how the resolver picks among candidate versions.
/// </summary>
[Flags]
public enum PluginVersionPreference
{
    /// <summary>Pick the highest stable version satisfying every constraint.</summary>
    PreferNewest = 0,

    /// <summary>Pick the lowest stable version satisfying every constraint.</summary>
    PreferOldest = 1 << 0,

    /// <summary>Include pre-release versions in the candidate pool.</summary>
    AllowPrereleases = 1 << 1,
}

/// <summary>
/// Immutable bag of knobs that govern plugin resolution behaviour.
/// </summary>
public sealed record PluginResolutionPolicy
{
    /// <summary>Version selection strategy.</summary>
    public PluginVersionPreference VersionPreference { get; init; } =
        PluginVersionPreference.PreferNewest;

    /// <summary>
    /// When <c>true</c>, resolution continues past the first error and
    /// accumulates all problems before throwing.
    /// When <c>false</c>, the first error throws immediately.
    /// </summary>
    public bool AccumulateErrors { get; init; } = true;

    /// <summary>Sensible defaults: newest stable, accumulate all errors.</summary>
    public static PluginResolutionPolicy Default { get; } = new();
}
