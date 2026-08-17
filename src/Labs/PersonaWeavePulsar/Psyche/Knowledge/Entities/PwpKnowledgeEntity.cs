using System;
using System.Collections.Generic;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.Internal;
using VK.Labs.PersonaWeavePulsar.Features.KnowledgeBook.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

/// <summary>
/// Database Entity representing a record in the VK_AI_Knowledge_Entries SQLite table.
/// </summary>
public sealed class PwpKnowledgeEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKKnowledgeId Id { get; set; }
    public required PwpKnowledgeBookId KnowledgeBookId { get; set; }
    public VKKnowledgeTriggerType TriggerType { get; set; } = VKKnowledgeTriggerType.Constant;
    public VKKnowledgeFilterLogic FilterLogic { get; set; } = VKKnowledgeFilterLogic.AndAny;
    public int StickyTurns { get; set; }
    public int CooldownTurns { get; set; }
    public int DelayTurns { get; set; }
    public string? ExclusiveGroup { get; set; }
    public int? ExclusiveWeight { get; set; }
    public string? Tag { get; set; }
    public string? StateConditions { get; set; }
    public double Probability { get; set; }
    public int? MaxCount { get; set; }
    public int? MaxCountPerTurn { get; set; }
    public int? StartTurn { get; set; }
    public int? EndTurn { get; set; }
    public string? ExclusionTag { get; set; }
    public string? DependencyId { get; set; }
    public string? ConflictGroupId { get; set; }
    public int? MinAffection { get; set; }
    public int? MaxAnger { get; set; }
    public string? RevealSecretKey { get; set; }
    public string? TargetPersonaId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? UserSegment { get; set; }
    public PwpPromptSegment Segment { get; set; } = new();
    public ICollection<PwpKnowledgeKeyEntity> Keys { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
