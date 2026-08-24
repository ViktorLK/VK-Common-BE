using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a chat session thread.
/// Pure persistence model for Psyche IVKSessionStore. [CS.05] [CS.08]
/// </summary>
public sealed class VKPsycheSessionEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKSessionId Id { get; set; }
    public VKSessionMode Mode { get; set; } = VKSessionMode.Isolated;
    public VKSessionId? ParentSessionId { get; set; }
    public VKSessionId? ForkSourceSessionId { get; set; }
    public string? ForkPointRef { get; set; }
    public VKSessionStatus Status { get; set; } = VKSessionStatus.Active;
    public int TurnCount { get; set; }
    public string? KnowledgeStateJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }

    public VKSessionKnowledgeState? ToKnowledgeState(IVKJsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(KnowledgeStateJson))
            return null;

        var dto = serializer.DeserializeOrDefault<SessionKnowledgeStateDto>(KnowledgeStateJson, new());
        if (dto is null)
            return null;

        var map = new Dictionary<VKKnowledgeId, int>();
        if (dto.LastTriggeredTurns is not null)
        {
            foreach (var kvp in dto.LastTriggeredTurns)
            {
                if (Guid.TryParse(kvp.Key, out var g))
                {
                    map[new VKKnowledgeId(g)] = kvp.Value;
                }
            }
        }

        return new VKSessionKnowledgeState
        {
            LastEvaluatedTurn = dto.LastEvaluatedTurn,
            LastTriggeredTurns = map
        };
    }

    private sealed record SessionKnowledgeStateDto
    {
        public int LastEvaluatedTurn { get; init; }
        public Dictionary<string, int>? LastTriggeredTurns { get; init; }
    }
}
