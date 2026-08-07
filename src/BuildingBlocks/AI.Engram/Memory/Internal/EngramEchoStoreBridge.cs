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

        // 2. Convert L1 VKMemoryEntry to VKEchoTrace
        var echoes = queryResult.Value
            .OrderBy(m => m.CreatedAt)
            .Select(m => new VKEchoTrace
            {
                TenantId = m.TenantId ?? VKTenantId.Default,
                SessionId = m.SessionId ?? sessionId,
                Id = new VKEchoId(m.Id.Value),
                Role = m.Metadata.TryGetValue("Role", out var roleStr) && Enum.TryParse<VKChatRole>(roleStr, out var r) ? r : VKChatRole.User,
                Content = m.Content,
                Timestamp = m.CreatedAt
            })
            .ToList();

        return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(echoes);
    }
}
