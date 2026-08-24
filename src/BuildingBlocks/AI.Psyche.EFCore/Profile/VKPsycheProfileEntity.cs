using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a User Profile Presence in Psyche.
/// Follows CS.05, CS.08.
/// </summary>
public sealed class VKPsycheProfileEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKProfileId Id { get; set; }
    public string? DisplayName { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferencesJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
