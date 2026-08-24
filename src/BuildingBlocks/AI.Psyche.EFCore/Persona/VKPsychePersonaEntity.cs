using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a VK AI Persona.
/// Pure persistence model for Psyche IVKPersonaStore. [CS.05] [CS.08]
/// </summary>
public sealed class VKPsychePersonaEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKPersonaId Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Personality { get; set; }
    public string? Scenario { get; set; }
    public string? FirstMessage { get; set; }
    public string? DialogueExamples { get; set; }
    public string? Traits { get; set; }
    public VKDirectiveId? DirectiveId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
