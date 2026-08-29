using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheDirectiveRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryDirectiveStore : IVKPsycheDirectiveRepository
{
    private readonly ConcurrentDictionary<VKDirectiveId, VKDirectiveCharter> _store = new();
    private readonly ILogger<InMemoryDirectiveStore> _logger;

    public InMemoryDirectiveStore(ILogger<InMemoryDirectiveStore> logger)
    {
        _logger = VKGuard.NotNull(logger);
        _logger.DirectiveInitialized();
    }

    public Task<VKResult<VKDirectiveCharter>> FindByIdAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound));
        }

        if (_store.TryGetValue(id, out var directive))
        {
            _logger.DirectiveResolved(id.ToString());
            return Task.FromResult(VKResult.Success(directive));
        }

        return Task.FromResult(VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> ListByIdsAsync(
        IReadOnlyList<VKDirectiveId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKDirectiveCharter>(ids.Count);
        foreach (var id in ids)
        {
            if (_store.TryGetValue(id, out var directive))
            {
                list.Add(directive);
                _logger.DirectiveResolved(id.ToString());
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKDirectiveCharter>>(list));
    }

    public Task<bool> ExistsAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _store.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKDirectiveCharter item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKDirectiveErrors.AlreadyExists));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKDirectiveCharter item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKDirectiveErrors.NotFound));
        }

        _store[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_store.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKDirectiveErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemoryDirectiveStore Seed(VKDirectiveCharter directive)
    {
        VKGuard.NotNull(directive);
        _store[directive.Id] = directive;
        return this;
    }

    public InMemoryDirectiveStore Seed(IEnumerable<VKDirectiveCharter> directives)
    {
        VKGuard.NotNull(directives);
        foreach (var d in directives)
        {
            _store[d.Id] = d;
        }
        return this;
    }

    public InMemoryDirectiveStore Remove(VKDirectiveId directiveId)
    {
        _store.TryRemove(directiveId, out _);
        return this;
    }

    public InMemoryDirectiveStore Clear()
    {
        _store.Clear();
        return this;
    }
}
