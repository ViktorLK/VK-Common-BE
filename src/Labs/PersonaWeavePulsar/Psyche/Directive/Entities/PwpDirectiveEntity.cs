using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;

/// <summary>
/// Database entity representing a tenant directive charter.
/// </summary>
public sealed class PwpDirectiveEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKDirectiveId Id { get; set; }
    public string? BehaviorRules { get; set; }
    public string? SafetyRules { get; set; }
    public string? OutputConstraints { get; set; }
    public string? Overview { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
