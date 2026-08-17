using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

/// <summary>
/// Database entity representing a record in the VK_AI_Knowledge_Keys SQLite table.
/// </summary>
public sealed class PwpKnowledgeKeyEntity : IVKMultiTenantEntity
{
    public VKTenantId? TenantId { get; set; }
    public required Guid Id { get; set; }
    public required VKKnowledgeId KnowledgeEntryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public VKKnowledgeMatchType MatchType { get; set; } = VKKnowledgeMatchType.Contains;
    public bool IsFilter { get; set; }
    public bool CaseSensitive { get; set; }

    public PwpKnowledgeEntity? Entry { get; set; }
}
