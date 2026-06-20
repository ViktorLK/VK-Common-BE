using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Default implementation of the Tenant Directive resolver.
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

    public Task<VKResult<VKDirectiveCharter>> GetDirectiveAsync(
        VKDirectiveId directiveId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotEmptyGuid(directiveId.Value);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(directiveId, out var directive))
        {
            return Task.FromResult(VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound));
        }

        _logger.DirectiveResolved(directiveId.ToString());

        return Task.FromResult(VKResult.Success(directive));
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
