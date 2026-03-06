using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins.Resolution;

/// <summary>
/// Pure resolution engine: given a catalogue of available manifests and a set
/// of root requests, produces a dependency-ordered list of manifests to load.
///
/// Stateless — all inputs go in via <see cref="Resolve"/>, so the same
/// instance (or a static call) can be reused for download planning, dry-run
/// checks, etc.
/// </summary>
public sealed class PluginResolver
{
    /// <summary>
    /// Result of a successful resolution: manifests in dependency-first
    /// (topological) order plus the full resolved map for introspection.
    /// </summary>
    public sealed record ResolutionResult(
        IReadOnlyList<PluginManifest> OrderedManifests,
        IReadOnlyDictionary<PluginId, PluginManifest> ResolvedMap
    );

    /// <summary>
    /// Resolves <paramref name="requests"/> against the supplied
    /// <paramref name="catalogue"/> using the given <paramref name="policy"/>.
    /// </summary>
    /// <param name="catalogue">
    /// Every known manifest, keyed by plugin id. Typically comes straight from
    /// <see cref="IPluginHostBuilder.AvailablePlugins"/> or from a remote
    /// package index.
    /// </param>
    /// <param name="requests">
    /// Root plugins the caller wants loaded. Each carries a
    /// <see cref="SemVersionRange"/> that constrains which versions are
    /// acceptable.
    /// </param>
    /// <param name="policy">Resolution knobs. Pass <c>null</c> for defaults.</param>
    /// <returns>Dependency-ordered manifests ready to load or download.</returns>
    /// <exception cref="PluginResolutionException">
    /// Thrown when one or more constraints cannot be satisfied or cycles exist.
    /// </exception>
    public ResolutionResult Resolve(
        IReadOnlyDictionary<PluginId, IReadOnlyList<PluginManifest>> catalogue,
        IReadOnlyList<PluginRequest> requests,
        PluginResolutionPolicy? policy = null
    )
    {
        policy ??= PluginResolutionPolicy.Default;

        var errors = new List<string>();
        var resolved = new Dictionary<PluginId, PluginManifest>();

        // ── Phase 1: resolve the full dependency graph ────────────────────────
        //
        // Each root request specifies a PluginId + VersionRange.  We pick the
        // best candidate from the catalogue that satisfies the range (respecting
        // the policy's version preference and pre-release flag).
        //
        // Transitive dependencies are resolved the same way.  If a plugin is
        // already resolved and a later requester imposes a range the chosen
        // version does not satisfy, we record a conflict and keep going (when
        // AccumulateErrors is true) so the caller sees every problem at once.

        // Seed with root requests.
        foreach (var request in requests)
        {
            if (!catalogue.TryGetValue(request.Id, out var manifests))
            {
                AddError(
                    errors,
                    policy,
                    $"Requested plugin '{request.Id.Value}' is not available."
                );
                continue;
            }

            var candidate = PickBest(manifests, request.VersionRange, policy);

            if (candidate is null)
            {
                AddError(
                    errors,
                    policy,
                    $"Requested plugin '{request.Id.Value}' {request.VersionRange} "
                        + $"has no matching version "
                        + $"(available: {FormatVersions(manifests)})."
                );
                continue;
            }

            if (resolved.TryGetValue(request.Id, out var existing))
            {
                // Two root requests for the same plugin — fine if they resolved
                // to the same version, error otherwise.
                if (existing.Version != candidate.Version)
                {
                    AddError(
                        errors,
                        policy,
                        $"Conflicting root requests for '{request.Id.Value}': "
                            + $"v{existing.Version} and v{candidate.Version} both match "
                            + $"different root constraints."
                    );
                }
                continue;
            }

            resolved[request.Id] = candidate;
        }

        // BFS over dependency edges.
        var visited = new HashSet<PluginId>();
        var queue = new Queue<PluginManifest>(resolved.Values);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current.Id))
            {
                continue;
            }

            foreach (var dep in current.Dependencies)
            {
                if (!catalogue.TryGetValue(dep.Id, out var depManifests))
                {
                    AddError(
                        errors,
                        policy,
                        $"Plugin '{current.Id.Value}' v{current.Version} requires "
                            + $"'{dep.Id.Value}' {dep.VersionRange}, which is not available."
                    );
                    continue;
                }

                if (resolved.TryGetValue(dep.Id, out var alreadyResolved))
                {
                    if (!dep.VersionRange.Contains(alreadyResolved.Version))
                    {
                        AddError(
                            errors,
                            policy,
                            $"Plugin '{current.Id.Value}' v{current.Version} requires "
                                + $"'{dep.Id.Value}' {dep.VersionRange}, but resolved version "
                                + $"v{alreadyResolved.Version} does not satisfy that range."
                        );
                    }
                    continue;
                }

                // Intersect this dep's range with any future narrowing — for now
                // we just pick from the dep's own range.
                var candidate = PickBest(depManifests, dep.VersionRange, policy);

                if (candidate is null)
                {
                    AddError(
                        errors,
                        policy,
                        $"Plugin '{current.Id.Value}' v{current.Version} requires "
                            + $"'{dep.Id.Value}' {dep.VersionRange}, but no available version "
                            + $"satisfies that range "
                            + $"(available: {FormatVersions(depManifests)})."
                    );
                    continue;
                }

                resolved[dep.Id] = candidate;
                queue.Enqueue(candidate);
            }
        }

        ThrowIfErrors(errors);

        // ── Phase 2: topological sort + cycle detection ───────────────────────

        var sorted = new List<PluginManifest>(resolved.Count);
        var permanent = new HashSet<PluginId>(resolved.Count);
        var temporary = new HashSet<PluginId>(resolved.Count);
        var pathStack = new List<PluginId>();

        void Visit(PluginId id)
        {
            if (permanent.Contains(id))
            {
                return;
            }

            if (temporary.Contains(id))
            {
                var cycleStart = pathStack.IndexOf(id);
                var cyclePath = pathStack.Skip(cycleStart).Append(id).Select(p => p.Value);
                errors.Add($"Circular dependency detected: {string.Join(" → ", cyclePath)}");
                return;
            }

            if (!resolved.TryGetValue(id, out var manifest))
            {
                return;
            }

            temporary.Add(id);
            pathStack.Add(id);

            foreach (var dep in manifest.Dependencies)
            {
                Visit(dep.Id);
            }

            pathStack.RemoveAt(pathStack.Count - 1);
            temporary.Remove(id);
            permanent.Add(id);
            sorted.Add(manifest);
        }

        foreach (var id in resolved.Keys)
        {
            Visit(id);
        }

        ThrowIfErrors(errors);

        return new ResolutionResult(sorted, resolved);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Picks the best manifest from <paramref name="manifests"/> that falls
    /// inside <paramref name="range"/>, honouring the policy's version
    /// preference and pre-release flag.
    /// </summary>
    private static PluginManifest? PickBest(
        IReadOnlyList<PluginManifest> manifests,
        SemVersionRange range,
        PluginResolutionPolicy policy
    )
    {
        var candidates = manifests.Where(m => range.Contains(m.Version));

        if (!policy.VersionPreference.HasFlag(PluginVersionPreference.AllowPrereleases))
        {
            candidates = candidates.Where(m => !m.Version.IsPrerelease);
        }

        var ordered = policy.VersionPreference.HasFlag(PluginVersionPreference.PreferOldest)
            ? candidates.OrderBy(m => m.Version, SemVersion.PrecedenceComparer)
            : candidates.OrderByDescending(m => m.Version, SemVersion.PrecedenceComparer);

        return ordered.FirstOrDefault();
    }

    private static void AddError(List<string> errors, PluginResolutionPolicy policy, string message)
    {
        if (policy.AccumulateErrors)
        {
            errors.Add(message);
        }
        else
        {
            throw new PluginResolutionException(new[] { message });
        }
    }

    private static void ThrowIfErrors(List<string> errors)
    {
        if (errors.Count > 0)
        {
            throw new PluginResolutionException(errors);
        }
    }

    private static string FormatVersions(IReadOnlyList<PluginManifest> manifests) =>
        string.Join(", ", manifests.Select(m => m.Version));
}
