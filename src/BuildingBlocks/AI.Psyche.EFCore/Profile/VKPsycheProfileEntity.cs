using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a User Profile Presence in Psyche.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKProfilePresence), TableName = "VK_AI_Psyche_Profile", FlattenBy = [nameof(VKProfilePresence.Coordinates)])]
public sealed class VKPsycheProfileEntity : IVKTenantScoped, IVKAuditable
{
    /// <inheritdoc />
    [VKPersistIndex]
    public VKTenantId TenantId { get; set; }

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
    /// Gets or sets the custom user description, background, and prompt instructions.
    /// </summary>
    [MaxLength(16000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the preferred addressing term or honorific for the user.
    /// </summary>
    [MaxLength(64)]
    public string? AddressingTerm { get; set; }

    /// <summary>
    /// Gets or sets the overall communication tone.
    /// </summary>
    public VKInteractionTone? InteractionTone { get; set; }

    /// <summary>
    /// Gets or sets the response verbosity level.
    /// </summary>
    public VKResponseVerbosity? ResponseVerbosity { get; set; }

    /// <summary>
    /// Gets or sets the emoji policy.
    /// </summary>
    public VKEmojiPolicy? EmojiPolicy { get; set; }

    /// <summary>
    /// Gets or sets the target chat role when this profile is rendered in prompt context.
    /// </summary>
    public VKChatRole Role { get; set; } = VKChatRole.System;

    /// <summary>
    /// Gets or sets the timeline position depth in prompt assembly if specified.
    /// </summary>
    public int? TimelineDepth { get; set; }

    /// <summary>
    /// Gets or sets the relative position section (e.g. SystemTop, BeforeDirective, AfterPersona).
    /// </summary>
    public VKPromptRelativeDepth? RelativeDepth { get; set; }

    /// <summary>
    /// Gets or sets the tie-breaking priority when multiple segments share the same position.
    /// </summary>
    public int DepthPriority { get; set; }

    /// <summary>
    /// Gets or sets the optional XML wrapper tag name when injected into prompt context.
    /// </summary>
    [MaxLength(64)]
    public string? TagName { get; set; }

    /// <summary>
    /// Gets or sets the precalculated or estimated token count for this profile presence segment.
    /// </summary>
    public int TokenCount { get; set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }
}
