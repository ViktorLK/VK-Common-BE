using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Memory.Internal;

/// <summary>
/// Bridge implementing Psyche's <see cref="IVKEchoStore"/> by querying Engram's L1 <see cref="VKEchoEngramEntry"/>.
/// Supports single-level ParentSessionId ancestry tracing for <see cref="VKSessionMode.Continuous"/> mode.
/// Provides zero physical data redundancy while maintaining complete module decoupling.
/// Follows AP.01 and BB.01.
/// </summary>
internal sealed class EngramEchoStoreBridge : IVKEchoStore
{
    private readonly IVKMemoryStore _memoryStore;
    private readonly IVKGuidGenerator _guidGenerator;

    public EngramEchoStoreBridge(IVKMemoryStore memoryStore, IVKGuidGenerator guidGenerator)
    {
        _memoryStore = VKGuard.NotNull(memoryStore);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch current session L1 memories
        var queryResult = await _memoryStore.QueryAsync(new VKMemoryQuery
        {
            SessionId = sessionId,
            Category = VKMemoryCategory.ShortTerm,
            TopK = 100
        }, cancellationToken).ConfigureAwait(false);

        if (queryResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyCollection<VKEchoTrace>>(queryResult.Errors);
        }

        var currentEntries = queryResult.Value.ToList();

        var echoes = currentEntries.Select(e => new VKEchoTrace
        {
            Role = e.Metadata.TryGetValue("Role", out var role) && role == "User"
                ? VKChatRole.User
                : VKChatRole.Assistant,
            Content = e.Content,
            Timestamp = e.CreatedAt
        }).ToList();

        return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(echoes);
    }

    public async Task<VKResult<VKSessionId?>> GetParentSessionIdAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var queryResult = await _memoryStore.QueryAsync(new VKMemoryQuery
        {
            SessionId = sessionId,
            Category = VKMemoryCategory.ShortTerm,
            TopK = 1
        }, cancellationToken).ConfigureAwait(false);

        if (queryResult.IsSuccess && queryResult.Value.Count > 0)
        {
            var firstEntry = queryResult.Value[0];
            if (firstEntry.Metadata.TryGetValue("ParentSessionId", out var parentStr) && Guid.TryParse(parentStr, out var parentGuid))
            {
                return VKResult.Success<VKSessionId?>(new VKSessionId(parentGuid));
            }
        }

        return VKResult.Success<VKSessionId?>(null);
    }

    public async Task<VKResult> AppendHistoryAsync(
        VKSessionId sessionId,
        IEnumerable<VKEchoTrace> traces,
        CancellationToken cancellationToken = default)
    {
        var entries = traces.Select(t => new VKMemoryEntry
        {
            Id = VKMemoryId.New(_guidGenerator),
            SessionId = sessionId,
            Category = VKMemoryCategory.ShortTerm,
            Content = t.Content,
            CreatedAt = t.Timestamp,
            Metadata = new Dictionary<string, string>
            {
                ["Role"] = t.Role.ToString()
            }.ToFrozenDictionary()
        });

        return await _memoryStore.UpsertBatchAsync(entries, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ClearHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var historyResult = await _memoryStore.QueryAsync(new VKMemoryQuery
        {
            SessionId = sessionId,
            Category = VKMemoryCategory.ShortTerm,
            TopK = 1000
        }, cancellationToken).ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return VKResult.Success();
        }

        foreach (var entry in historyResult.Value)
        {
            await _memoryStore.DeleteAsync(entry.Id, entry.TenantId, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
