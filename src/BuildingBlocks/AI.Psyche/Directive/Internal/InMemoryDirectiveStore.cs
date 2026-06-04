using System;
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
    private readonly ConcurrentDictionary<string, VKDirectiveCharter> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<InMemoryDirectiveStore> _logger;

    public InMemoryDirectiveStore(ILogger<InMemoryDirectiveStore> logger)
    {
        _logger = VKGuard.NotNull(logger);

        DirectiveDiagnostics.DirectiveInitialized(_logger);
    }

    public Task<VKResult<VKDirectiveCharter>> GetDirectiveAsync(
        string directiveId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(directiveId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(directiveId, out var directive))
        {
            return Task.FromResult(VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound));
        }

        DirectiveDiagnostics.DirectiveResolved(_logger, directiveId);

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

    public InMemoryDirectiveStore Remove(string directiveId)
    {
        VKGuard.NotNullOrWhiteSpace(directiveId);
        
        _store.TryRemove(directiveId, out _);

        return this;
    }

    public InMemoryDirectiveStore Clear()
    {
        _store.Clear();

        return this;
    }
}
