namespace Aitive.Framework.Plugins;

/// <summary>
/// Thrown when plugin resolution fails. Accumulates all errors rather than
/// stopping at the first failure.
/// </summary>
public sealed class PluginResolutionException : PluginException
{
    public IReadOnlyList<string> Errors { get; }

    public PluginResolutionException(IReadOnlyList<string> errors)
        : base(FormatMessage(errors))
    {
        Errors = errors;
    }

    private static string FormatMessage(IReadOnlyList<string> errors) =>
        $"Plugin resolution failed with {errors.Count} error(s):{Environment.NewLine}"
        + string.Join(Environment.NewLine, errors.Select((e, i) => $"  {i + 1}. {e}"));
}
