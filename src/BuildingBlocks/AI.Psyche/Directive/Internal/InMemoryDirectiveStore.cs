using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Default implementation of the Directive resolver.
/// Implements AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryDirectiveStore : IVKDirectiveStore
{
    private readonly ConcurrentDictionary<VKDirectiveId, VKDirectiveCharter> _store = new();
    private readonly ILogger<InMemoryDirectiveStore> _logger;

    public InMemoryDirectiveStore(ILogger<InMemoryDirectiveStore> logger)
    {
        _logger = VKGuard.NotNull(logger);

        _logger.DirectiveInitialized();
    }

    public Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> GetDirectivesAsync(
        IReadOnlyList<VKDirectiveId> directiveIds,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(directiveIds);
        cancellationToken.ThrowIfCancellationRequested();

        var list = new List<VKDirectiveCharter>(directiveIds.Count);
        foreach (var directiveId in directiveIds)
        {
            if (_store.TryGetValue(directiveId, out var directive))
            {
                list.Add(directive);
                _logger.DirectiveResolved(directiveId.ToString());
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKDirectiveCharter>>(list));
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
        VKGuard.NotEmptyGuid(directiveId.Value);

        _store.TryRemove(directiveId, out _);

        return this;
    }

    public InMemoryDirectiveStore Clear()
    {
        _store.Clear();

        return this;
    }
}
