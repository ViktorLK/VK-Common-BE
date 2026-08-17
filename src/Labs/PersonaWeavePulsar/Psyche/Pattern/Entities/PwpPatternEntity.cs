using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.Internal;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;

/// <summary>
/// Database Entity representing a record in the VK_AI_Tenant_Preset_Pattern SQLite table.
/// </summary>
public sealed class PwpPatternEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKPatternId Id { get; set; }
    public PwpPromptSegment Segment { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
