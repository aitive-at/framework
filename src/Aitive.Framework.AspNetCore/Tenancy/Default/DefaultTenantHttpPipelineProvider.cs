using System.Collections.Concurrent;
using System.Diagnostics;
using Aitive.Framework.Collections;
using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Tenancy.Default;

internal sealed class DefaultTenantHttpPipelineEntry
{
    private readonly Lock _stateLock = new();

    private readonly SemaphoreSlim _initGate = new(1, 1);

    private ITenantHttpPipeline? _innerPipeline;
    private Task<ITenantHttpPipeline>? _initTask;
    private int _referenceCount;
    private bool _markedForDisposal;

    internal TenantId TenantId { get; }

    // Used by the LRU list in the provider. Protected by the provider's cache lock.
    internal LinkedListNode<DefaultTenantHttpPipelineEntry>? LruNode { get; set; }

    internal DefaultTenantHttpPipelineEntry(TenantId tenantId)
    {
        TenantId = tenantId;
    }

    internal bool TryAddReference()
    {
        lock (_stateLock)
        {
            if (_markedForDisposal)
            {
                return false;
            }

            _referenceCount++;
            return true;
        }
    }

    internal bool Release()
    {
        lock (_stateLock)
        {
            Debug.Assert(_referenceCount > 0, "Release called with zero references.");
            _referenceCount--;
            return _referenceCount == 0 && _markedForDisposal;
        }
    }

    internal bool MarkForDisposal()
    {
        lock (_stateLock)
        {
            if (_markedForDisposal)
            {
                return false; // Already marked — someone else will handle it.
            }

            _markedForDisposal = true;
            return _referenceCount == 0;
        }
    }

    internal async ValueTask<ITenantHttpPipeline> GetOrCreatePipeline(
        Func<TenantId, HttpContext, Task<ITenantHttpPipeline>> factory,
        HttpContext context
    )
    {
        // Fast path: already initialized.
        if (Volatile.Read(ref _innerPipeline) is { } existing)
        {
            return existing;
        }

        await _initGate.WaitAsync();

        try
        {
            // Double-check after acquiring the gate.
            if (_innerPipeline is not null)
            {
                return _innerPipeline;
            }

            // Start initialization — cache the task so any concurrent waiters
            // (if we chose to let them through) would await the same thing.
            _initTask = factory(TenantId, context);
            _innerPipeline = await _initTask;
            return _innerPipeline;
        }
        finally
        {
            _initGate.Release();
        }
    }

    internal async ValueTask DisposeInnerAsync()
    {
        if (_innerPipeline is not null)
        {
            await _innerPipeline.DisposeAsync();
            _innerPipeline = null;
        }

        _initGate.Dispose();
    }
}

internal sealed class DefaultTenantHttpPipelineHandle : ITenantHttpPipeline
{
    private readonly DefaultTenantHttpPipelineEntry _entry;
    private readonly ITenantHttpPipeline _innerPipeline;
    private int _disposed;

    internal DefaultTenantHttpPipelineHandle(
        DefaultTenantHttpPipelineEntry entry,
        ITenantHttpPipeline innerPipeline
    )
    {
        _entry = entry;
        _innerPipeline = innerPipeline;
    }

    public TenantId Id => _entry.TenantId;
    public IServiceProvider Services => _innerPipeline.Services;

    public Task Invoke(HttpContext context) => _innerPipeline.Invoke(context);

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        if (_entry.Release())
        {
            // We were the last reference and the entry has been evicted/invalidated.
            // We are responsible for cleaning up the inner pipeline.
            await _entry.DisposeInnerAsync();
        }
    }
}

internal sealed class DefaultTenantHttpPipeline : ITenantHttpPipeline
{
    private readonly AsyncServiceScope _tenantServiceScope;
    private readonly ITenantHttpRouter _tenantHttpRouter;
    private readonly RequestDelegate _invocationDelegate;

    internal DefaultTenantHttpPipeline(
        TenantId id,
        AsyncServiceScope tenantServiceScope,
        ITenantHttpRouter tenantHttpRouter,
        RequestDelegate invocationDelegate
    )
    {
        Id = id;

        _tenantServiceScope = tenantServiceScope;
        _tenantHttpRouter = tenantHttpRouter;
        _invocationDelegate = invocationDelegate;
    }

    public TenantId Id { get; }
    public IServiceProvider Services => _tenantServiceScope.ServiceProvider;

    public async Task Invoke(HttpContext context)
    {
        using var _ = await _tenantHttpRouter.Rewrite(context, Id);
        await _invocationDelegate.Invoke(context);
    }

    public ValueTask DisposeAsync()
    {
        return _tenantServiceScope.DisposeAsync();
    }
}

internal sealed class DefaultTenantHttpPipelineProvider
    : ITenantHttpPipelineProvider,
        IAsyncDisposable
{
    private readonly ITenantHttpRouter _tenantHttpRouter;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _maxCachedPipelines;

    // Cache state — all access protected by _cacheLock.
    // The lock is only held for fast, in-memory operations (dictionary + linked list),
    // never across async boundaries.
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<TenantId, DefaultTenantHttpPipelineEntry> _cache = new();
    private readonly LinkedList<DefaultTenantHttpPipelineEntry> _lruList = new(); // Head = most recent
    private bool _disposed;

    internal DefaultTenantHttpPipelineProvider(
        ITenantHttpRouter tenantHttpRouter,
        // We need the ROOT service provider. The HttpContext service provider would already be per request so that would
        // not produce the correct hierarchy.
        IServiceProvider serviceProvider,
        int maxCachedPipelines = 1000
    )
    {
        _tenantHttpRouter = tenantHttpRouter;
        _serviceProvider = serviceProvider;
        _maxCachedPipelines = maxCachedPipelines;
    }

    // ── ITenantHttpPipelineProvider ─────────────────────────────────────

    public async ValueTask<Optional<ITenantHttpPipeline>> GetOrCreate(HttpContext context)
    {
        var tenantIdOpt = await _tenantHttpRouter.Route(context);

        if (!tenantIdOpt.HasValue)
        {
            return Optional<ITenantHttpPipeline>.None;
        }

        var tenantId = tenantIdOpt.Value;

        // Fast path: try to get an existing, non-evicted entry.
        var (entry, needsEviction) = GetOrCreateEntry(tenantId);

        // Evict outside the cache lock to avoid holding it during disposal.
        if (needsEviction)
        {
            await EvictLeastRecentlyUsedAsync();
        }

        try
        {
            // Initialize the inner pipeline (no-op if already initialized).
            var innerPipeline = await entry.GetOrCreatePipeline(CreatePipeline, context);
            return Optional.Some<ITenantHttpPipeline>(
                new DefaultTenantHttpPipelineHandle(entry, innerPipeline)
            );
        }
        catch
        {
            // Initialization failed — release our reference so the entry can be cleaned up.
            if (entry.Release())
            {
                await entry.DisposeInnerAsync();
            }

            throw;
        }
    }

    public async ValueTask Invalidate(TenantId tenantId)
    {
        DefaultTenantHttpPipelineEntry? entry;

        lock (_cacheLock)
        {
            if (!_cache.Remove(tenantId, out entry))
            {
                return;
            }

            RemoveFromLru(entry);
        }

        // Mark outside the lock.
        if (entry.MarkForDisposal())
        {
            // No outstanding references — dispose immediately.
            await entry.DisposeInnerAsync();
        }
        // else: outstanding handles exist; the last PipelineHandle.DisposeAsync
        // will take care of cleanup.
    }

    // ── IAsyncDisposable ────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        List<DefaultTenantHttpPipelineEntry> entries;

        lock (_cacheLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            entries = new List<DefaultTenantHttpPipelineEntry>(_cache.Values);
            _cache.Clear();
            _lruList.Clear();
        }

        foreach (var entry in entries)
        {
            if (entry.MarkForDisposal())
            {
                await entry.DisposeInnerAsync();
            }
        }
    }

    private const string EndpointRouteBuilderKey = "__EndpointRouteBuilder";

    private async Task<ITenantHttpPipeline> CreatePipeline(TenantId tenantId, HttpContext context)
    {
        var tenantScope = _serviceProvider.CreateAsyncScope();
        var features =
            tenantScope.ServiceProvider.GetService<IServer>()?.Features ?? new FeatureCollection();

        var app = new ApplicationBuilder(tenantScope.ServiceProvider, features);

        app.UseRouting();

        // Try to retrieve the current 'IEndpointRouteBuilder'.
        if (
            !app.Properties.TryGetValue(EndpointRouteBuilderKey, out var obj)
            || obj is not IEndpointRouteBuilder routes
        )
        {
            throw new InvalidOperationException(
                "Failed to retrieve the current endpoint route builder."
            );
        }

        // Configure
        var moduleProviders = tenantScope.ServiceProvider.GetServices<ITenantHttpModuleProvider>();

        var modules = new List<IHttpModule>();

        foreach (var moduleProvider in moduleProviders)
        {
            modules.AddRange(await moduleProvider.GetHttpModules(tenantId, context.RequestAborted));
        }

        foreach (var module in modules.Ordered())
        {
            module.Register(tenantScope.ServiceProvider, app, routes);
        }

        // Disable additional endpoint forwarding
        app.UseEndpoints(_ => { });

        var invocationDelegate = app.Build();

        return new DefaultTenantHttpPipeline(
            tenantId,
            tenantScope,
            _tenantHttpRouter,
            invocationDelegate
        );
    }

    // ── Private helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Gets an existing entry or creates a new one. Always returns an entry
    /// with an incremented reference count. The bool indicates whether
    /// eviction is needed (cache was at or above capacity when a new entry was added).
    /// </summary>
    private (DefaultTenantHttpPipelineEntry Entry, bool NeedsEviction) GetOrCreateEntry(
        TenantId tenantId
    )
    {
        lock (_cacheLock)
        {
            ThrowIfDisposed();

            if (_cache.TryGetValue(tenantId, out var existing))
            {
                if (existing.TryAddReference())
                {
                    // Promote to most-recently-used.
                    PromoteInLru(existing);
                    return (existing, false);
                }

                // Entry was marked for disposal between our lookup and TryAddReference.
                // Remove it from the cache and fall through to create a new one.
                _cache.Remove(tenantId);
                RemoveFromLru(existing);
            }

            // Create a new entry.
            var entry = new DefaultTenantHttpPipelineEntry(tenantId);

            // TryAddReference will always succeed here (not yet marked for disposal).
            var added = entry.TryAddReference();
            Debug.Assert(added);

            _cache[tenantId] = entry;
            entry.LruNode = _lruList.AddFirst(entry);

            return (entry, _cache.Count > _maxCachedPipelines);
        }
    }

    /// <summary>
    /// Evicts the least-recently-used entry. Called outside the cache lock,
    /// re-acquires the lock to find the victim, then disposes if possible.
    /// </summary>
    private async ValueTask EvictLeastRecentlyUsedAsync()
    {
        DefaultTenantHttpPipelineEntry? victim = null;

        lock (_cacheLock)
        {
            // Walk from the tail (least recently used) to find an entry to evict.
            var node = _lruList.Last;
            while (node is not null && _cache.Count > _maxCachedPipelines)
            {
                victim = node.Value;
                var prev = node.Previous;

                _cache.Remove(victim.TenantId);
                _lruList.Remove(node);
                victim.LruNode = null;

                node = prev;
                break; // Evict one at a time; caller can loop if needed.
            }
        }

        if (victim is not null && victim.MarkForDisposal())
        {
            await victim.DisposeInnerAsync();
        }
    }

    private void PromoteInLru(DefaultTenantHttpPipelineEntry entry)
    {
        // Must be called under _cacheLock.
        if (entry.LruNode is not null && entry.LruNode != _lruList.First)
        {
            _lruList.Remove(entry.LruNode);
            _lruList.AddFirst(entry.LruNode);
        }
    }

    private void RemoveFromLru(DefaultTenantHttpPipelineEntry entry)
    {
        // Must be called under _cacheLock.
        if (entry.LruNode is not null)
        {
            _lruList.Remove(entry.LruNode);
            entry.LruNode = null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DefaultTenantHttpPipelineProvider));
        }
    }
}
