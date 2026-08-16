using System;
using VK.Blocks.Core;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Profile.Entities;

/// <summary>
/// Database entity representing a record in the VK_AI_Profile_Presence SQLite table.
/// Implements <see cref="IVKMultiTenantEntity"/> for SaaS tenant isolation.
/// Matches Psyche's <see cref="VKProfilePresence"/> domain naming.
/// </summary>
public sealed class PwpProfileEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKUserId UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferencesJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
