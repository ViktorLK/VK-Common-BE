using System;
using System.Collections.Generic;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;

/// <summary>
/// Database entity representing a record in the VK_AI_Chat_Sessions SQLite table.
/// </summary>
public sealed class PwpSessionEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKSessionId Id { get; set; }
    public VKUserId? UserId { get; set; }
    public required VKPersonaId PersonaId { get; set; }
    public VKSessionMode Mode { get; set; } = VKSessionMode.Isolated;
    public VKSessionId? ParentSessionId { get; set; }
    public VKSessionId? ForkSourceSessionId { get; set; }
    public string? ForkPointRef { get; set; }
    public VKSessionStatus Status { get; set; } = VKSessionStatus.Active;
    public int TurnCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public string? CustomModelId { get; set; }
    public string? CustomApiKey { get; set; }
    public string? CustomServiceType { get; set; }
    public string? CustomEndpoint { get; set; }
    public string? KnowledgeStateJson { get; set; }

    public ICollection<PwpEchoEntity> Messages { get; set; } = [];

    /// <summary>
    /// Deserializes KnowledgeStateJson into domain <see cref="VKSessionKnowledgeState"/>.
    /// </summary>
    public VKSessionKnowledgeState ToKnowledgeState(IVKJsonSerializer serializer)
    {
        return serializer.DeserializeOrDefault<VKSessionKnowledgeState>(KnowledgeStateJson) ?? new VKSessionKnowledgeState();
    }
}
