using Aitive.Framework.Collections;
using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins.Hosts;

public sealed class DefaultPluginHostBuilder : IPluginHostBuilder
{
    private readonly Dictionary<PluginId, IReadOnlyList<PluginManifest>> _availablePlugins;
    private readonly Dictionary<PluginVersionId, IPluginProvider> _pluginProviders;

    public DefaultPluginHostBuilder()
    {
        _availablePlugins = new Dictionary<PluginId, IReadOnlyList<PluginManifest>>();
        _pluginProviders = new Dictionary<PluginVersionId, IPluginProvider>();
    }

    public IReadOnlyDictionary<PluginId, IReadOnlyList<PluginManifest>> AvailablePlugins =>
        _availablePlugins;

    public IPluginHostBuilder WithProvider(IPluginProvider provider)
    {
        foreach (var pluginManifest in provider.AvailablePlugins)
        {
            var list =
                (IList<PluginManifest>)
                    _availablePlugins.GetOrAdd(pluginManifest.Id, _ => new List<PluginManifest>());

            list.Add(pluginManifest);
            _availablePlugins[pluginManifest.Id] = (IReadOnlyList<PluginManifest>)list;

            _pluginProviders[pluginManifest.VersionId] = provider;
        }

        return this;
    }

    public IPluginHost Build(IReadOnlyList<PluginVersionId> pluginsToLoad)
    {
        var orderedManifests = ResolveAndOrder(pluginsToLoad.ToList());
        var loadedPlugins = new List<IPlugin>();

        foreach (var manifest in orderedManifests)
        {
            var provider = _pluginProviders[manifest.VersionId];
            loadedPlugins.Add(provider.Load(manifest.VersionId));
        }

        return new DefaultPluginHost(loadedPlugins);
    }

    private IReadOnlyList<PluginManifest> ResolveAndOrder(
        IReadOnlyList<PluginVersionId> requestedPlugins
    )
    {
        var errors = new List<string>();

        // ── Phase 1: resolve the full dependency graph ────────────────────────────
        //
        // resolved  : PluginId -> the single manifest chosen for that plugin.
        // Requested plugins are pinned to their explicit version; transitive deps
        // are resolved to the highest version satisfying every constraint seen so far.
        //
        // When we encounter a plugin that is already resolved but a new requester
        // imposes a range that the chosen version does not satisfy, we record a
        // conflict error and continue — we do NOT re-resolve, so we can keep
        // collecting further errors in the same pass.

        var resolved = new Dictionary<PluginId, PluginManifest>();

        // Seed with the pinned requested plugins.
        foreach (var pvid in requestedPlugins)
        {
            if (!_availablePlugins.TryGetValue(pvid.PluginId, out var manifests))
            {
                errors.Add(
                    $"Requested plugin '{pvid.PluginId.Value}' v{pvid.Version} is not available."
                );
                continue;
            }

            var manifest = manifests.FirstOrDefault(m => m.Version == pvid.Version);
            if (manifest is null)
            {
                errors.Add(
                    $"Requested plugin '{pvid.PluginId.Value}' v{pvid.Version} was not found "
                        + $"(available: {string.Join(", ", manifests.Select(m => m.Version))})."
                );
                continue;
            }

            // Two requested entries for the same plugin with different versions.
            if (
                resolved.TryGetValue(pvid.PluginId, out var existing)
                && existing.Version != pvid.Version
            )
            {
                errors.Add(
                    $"Conflicting requested versions for '{pvid.PluginId.Value}': "
                        + $"v{existing.Version} and v{pvid.Version} were both explicitly requested."
                );
                continue;
            }

            resolved[pvid.PluginId] = manifest;
        }

        // BFS over the dependency graph starting from the seeded manifests.
        // visited prevents re-processing a manifest whose deps we have already walked.
        var visited = new HashSet<PluginId>();
        var queue = new Queue<PluginManifest>(resolved.Values);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!visited.Add(current.Id))
                continue;

            foreach (var dep in current.Dependencies)
            {
                if (!_availablePlugins.TryGetValue(dep.Id, out var depManifests))
                {
                    errors.Add(
                        $"Plugin '{current.Id.Value}' v{current.Version} requires "
                            + $"'{dep.Id.Value}' {dep.VersionRange}, which is not available at all."
                    );
                    continue;
                }

                if (resolved.TryGetValue(dep.Id, out var alreadyResolved))
                {
                    // The version was already chosen (either pinned or resolved earlier).
                    // Verify it satisfies this new requester's range.
                    if (!dep.VersionRange.Contains(alreadyResolved.Version))
                    {
                        errors.Add(
                            $"Plugin '{current.Id.Value}' v{current.Version} requires "
                                + $"'{dep.Id.Value}' {dep.VersionRange}, but the resolved version "
                                + $"v{alreadyResolved.Version} does not satisfy that range "
                                + $"(transitive conflict)."
                        );
                    }

                    // Regardless of the conflict, we do not re-resolve — we want to
                    // keep collecting errors for the rest of the graph.
                }
                else
                {
                    // Choose the highest version that satisfies the range.
                    var candidate = depManifests
                        .Where(m => dep.VersionRange.Contains(m.Version))
                        .OrderByDescending(m => m.Version, SemVersion.PrecedenceComparer)
                        .FirstOrDefault();

                    if (candidate is null)
                    {
                        errors.Add(
                            $"Plugin '{current.Id.Value}' v{current.Version} requires "
                                + $"'{dep.Id.Value}' {dep.VersionRange}, but no available version "
                                + $"satisfies that range "
                                + $"(available: {string.Join(", ", depManifests.Select(m => m.Version))})."
                        );
                        continue;
                    }

                    resolved[dep.Id] = candidate;
                    queue.Enqueue(candidate);
                }
            }
        }

        // Surface resolution errors before attempting the sort.
        if (errors.Count > 0)
        {
            throw new PluginResolutionException(errors);
        }

        // ── Phase 2: topological sort + cycle detection ───────────────────────────
        //
        // Classic DFS-based topo sort (Kahn's via DFS).
        // temporaryMark  = "currently on the DFS stack" → back-edge = cycle.
        // permanentMark  = fully processed, already added to sorted output.

        var sorted = new List<PluginManifest>(resolved.Count);
        var permanent = new HashSet<PluginId>(resolved.Count);
        var temporary = new HashSet<PluginId>(resolved.Count);
        var pathStack = new List<PluginId>(); // for human-readable cycle reporting

        void Visit(PluginId id)
        {
            if (permanent.Contains(id))
                return;

            if (temporary.Contains(id))
            {
                // Reconstruct the cycle from the current path stack.
                var cycleStart = pathStack.IndexOf(id);
                var cyclePath = pathStack
                    .Skip(cycleStart)
                    .Append(id) // close the loop
                    .Select(p => p.Value);

                errors.Add($"Circular dependency detected: {string.Join(" → ", cyclePath)}");
                return; // stop recursing into this branch
            }

            // The plugin may be absent from resolved if it had an earlier error —
            // skip it here so we don't report spurious follow-on errors.
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
            sorted.Add(manifest); // post-order = dependencies before dependents
        }

        foreach (var id in resolved.Keys)
        {
            Visit(id);
        }

        if (errors.Count > 0)
        {
            throw new PluginResolutionException(errors);
        }

        return sorted;
    }
}
