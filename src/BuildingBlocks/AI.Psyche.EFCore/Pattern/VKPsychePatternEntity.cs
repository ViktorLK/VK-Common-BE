using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a Pattern Entry (Few-Shot pattern/schema).
/// Follows CS.05, CS.08.
/// </summary>
public sealed class VKPsychePatternEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKPatternId Id { get; set; }
    public required string Content { get; set; }
    public string? Name { get; set; }
    public bool IsEnabled { get; set; } = true;
    public VKChatRole Role { get; set; } = VKChatRole.System;
    public int? AbsoluteDepth { get; set; }
    public VKPromptRelativeDepth? RelativeDepth { get; set; }
    public int DepthPriority { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
