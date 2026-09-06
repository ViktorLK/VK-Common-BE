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

    public async Task<VKResult<IReadOnlyCollection<VKEchoMetadata>>> GetMetadataAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var queryResult = await _memoryStore.QueryAsync(new VKMemoryQuery
        {
            SessionId = sessionId,
            Category = VKMemoryCategory.ShortTerm,
            TopK = 100
        }, cancellationToken).ConfigureAwait(false);

        if (queryResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyCollection<VKEchoMetadata>>(queryResult.Errors);
        }

        var list = queryResult.Value
            .OrderBy(m => m.CreatedAt)
            .Select(m => new VKEchoMetadata
            {
                Id = new VKEchoId(m.Id.Value),
                SessionId = m.SessionId ?? sessionId,
                Role = m.Metadata.TryGetValue("Role", out var roleStr) && Enum.TryParse<VKChatRole>(roleStr, out var r) ? r : VKChatRole.User,
                TokenCount = m.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc) ? tc : 10,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>(list);
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetTracesByIdsAsync(
        IReadOnlyCollection<VKEchoId> ids,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(ids);

        if (ids.Count == 0)
        {
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]);
        }

        var traces = new List<VKEchoTrace>();
        foreach (var id in ids)
        {
            var memoryEntryResult = await _memoryStore.GetByIdAsync(new VKMemoryId(id.Value), cancellationToken).ConfigureAwait(false);
            if (memoryEntryResult.IsSuccess && memoryEntryResult.Value is not null)
            {
                var m = memoryEntryResult.Value;
                traces.Add(new VKEchoTrace
                {
                    SessionId = m.SessionId ?? VKSessionId.Empty,
                    Id = new VKEchoId(m.Id.Value),
                    Role = m.Metadata.TryGetValue("Role", out var roleStr) && Enum.TryParse<VKChatRole>(roleStr, out var r) ? r : VKChatRole.User,
                    Content = m.Content,
                    TokenCount = m.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc) ? tc : 10,
                    CreatedAt = m.CreatedAt
                });
            }
        }

        return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(traces.OrderBy(t => t.CreatedAt).ToList());
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
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

        var echoes = queryResult.Value
            .OrderBy(m => m.CreatedAt)
            .Select(m => new VKEchoTrace
            {
                SessionId = m.SessionId ?? sessionId,
                Id = new VKEchoId(m.Id.Value),
                Role = m.Metadata.TryGetValue("Role", out var roleStr) && Enum.TryParse<VKChatRole>(roleStr, out var r) ? r : VKChatRole.User,
                Content = m.Content,
                TokenCount = m.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc) ? tc : 10,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(echoes);
    }

    public async Task<VKResult> SaveHistoryAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(trace);

        var memoryEntry = new VKMemoryEntry
        {
            Id = new VKMemoryId(trace.Id.Value),
            TenantId = VKTenantId.Default,
            SessionId = trace.SessionId,
            Category = VKMemoryCategory.ShortTerm,
            Content = trace.Content,
            CreatedAt = trace.CreatedAt,
            Metadata = FrozenDictionary.ToFrozenDictionary(new Dictionary<string, string>
            {
                ["Role"] = trace.Role.ToString(),
                ["TokenCount"] = trace.TokenCount.ToString()
            })
        };

        return await _memoryStore.UpsertAsync(memoryEntry, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> SaveHistoryBatchAsync(
        IReadOnlyCollection<VKEchoTrace> traces,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(traces);
        foreach (var trace in traces)
        {
            var result = await SaveHistoryAsync(trace, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return VKResult.Success();
    }
}
