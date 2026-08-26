using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a User Profile Presence in Psyche.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKProfilePresence), TableName = "VK_AI_Psyche_Profile")]
public sealed class VKPsycheProfileEntity : IVKTenantScoped, IVKAuditable
{
    /// <inheritdoc />
    [VKPersistIndex]
    public VKTenantId? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed profile identifier (typically 1-to-1 with VKUserId).
    /// </summary>
    [VKPersistKey]
    public required VKProfileId Id { get; set; }

    /// <summary>
    /// Gets or sets the user preferred display name.
    /// </summary>
    [MaxLength(128)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the user preferred language code (e.g. "en-US", "ja-JP").
    /// </summary>
    [MaxLength(32)]
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Gets or sets the user standard IANA or Windows time zone.
    /// </summary>
    [MaxLength(64)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets arbitrary key-value user preference settings for prompt personalizations.
    /// </summary>
    [VKPersistColumn(TypeName = "jsonb")]
    public IReadOnlyDictionary<string, string> Preferences { get; set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }
}
