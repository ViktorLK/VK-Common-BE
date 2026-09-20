using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a user's cognitive presence and personalized preferences in Psyche's execution pipeline.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKProfilePresence : VKAggregateRoot<VKProfileId> // [AP.01] sealed
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the user preferred display name.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the user preferred language code (e.g. "en-US", "ja-JP", "zh-CN").
    /// </summary>
    public string? PreferredLanguage { get; private set; }

    /// <summary>
    /// Gets the custom user description, background, and prompt instructions.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the preferred addressing term or honorific for the user (e.g. "王工", "Sensei", "您", "Dr. Smith").
    /// </summary>
    public string? AddressingTerm { get; private set; }

    /// <summary>
    /// Gets the optional overall communication tone and interaction posture (or null if unconfigured).
    /// </summary>
    public VKInteractionTone? InteractionTone { get; private set; }

    /// <summary>
    /// Gets the optional level of explanation detail and output verbosity (or null if unconfigured).
    /// </summary>
    public VKResponseVerbosity? ResponseVerbosity { get; private set; }

    /// <summary>
    /// Gets the optional emoji usage constraint for generated responses (or null if unconfigured).
    /// </summary>
    public VKEmojiPolicy? EmojiPolicy { get; private set; }

    /// <summary>
    /// Gets the relative position anchor in prompt assembly.
    /// Defaults to <see cref="VKPromptRelativeDepth.AfterDirective"/>.
    /// </summary>
    public VKPromptRelativeDepth? RelativeDepth { get; private set; } = VKPromptRelativeDepth.AfterDirective;

    /// <summary>
    /// Gets the rendering priority order among segments at the same relative depth.
    /// Priority must be between 0 and 999.
    /// Defaults to 10.
    /// </summary>
    public int DepthPriority { get; private set; } = 10;

    /// <summary>
    /// Gets the timeline depth (position relative to chat timeline) in the message layout if timeline positioning is used; otherwise, null.
    /// </summary>
    public int? TimelineDepth { get; private set; }

    /// <summary>
    /// Gets the optional XML wrapper tag name when injected into prompt context; or null to use system default.
    /// </summary>
    public string? TagName { get; private set; }

    /// <summary>
    /// Gets the precalculated or estimated token count for this profile presence.
    /// </summary>
    public int TokenCount { get; private set; } = 0;

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKProfilePresence(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? description,
        string? addressingTerm,
        VKInteractionTone? interactionTone,
        VKResponseVerbosity? responseVerbosity,
        VKEmojiPolicy? emojiPolicy,
        VKPromptRelativeDepth? relativeDepth,
        int depthPriority,
        int? timelineDepth,
        string? tagName,
        int tokenCount) : base(id)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        Description = description;
        AddressingTerm = addressingTerm;
        InteractionTone = interactionTone;
        ResponseVerbosity = responseVerbosity;
        EmojiPolicy = emojiPolicy;
        RelativeDepth = relativeDepth;
        DepthPriority = depthPriority;
        TimelineDepth = timelineDepth;
        TagName = tagName;
        TokenCount = tokenCount;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new profile presence aggregate root.
    /// </summary>
    public static VKResult<VKProfilePresence> Create(
        VKProfileId id,
        string? displayName = null,
        string? preferredLanguage = null,
        string? description = null,
        VKPromptRelativeDepth? relativeDepth = VKPromptRelativeDepth.AfterDirective,
        int depthPriority = 10,
        int? timelineDepth = null,
        string? tagName = null,
        int tokenCount = 0,
        string? addressingTerm = null,
        VKInteractionTone? interactionTone = null,
        VKResponseVerbosity? responseVerbosity = null,
        VKEmojiPolicy? emojiPolicy = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.InRange(depthPriority, 0, 999, nameof(depthPriority));

        return VKResult.Success(new VKProfilePresence(
            id,
            displayName,
            preferredLanguage,
            description,
            addressingTerm,
            interactionTone,
            responseVerbosity,
            emojiPolicy,
            relativeDepth,
            depthPriority,
            timelineDepth,
            tagName,
            Math.Max(0, tokenCount)));
    }

    /// <summary>
    /// Backward-compatibility factory overload with deprecated timeZone argument.
    /// </summary>
    [Obsolete("TimeZone has moved to AI.Cognitive.Presence.")]
    public static VKResult<VKProfilePresence> Create(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? timeZone,
        string? description,
        VKPromptRelativeDepth? relativeDepth = VKPromptRelativeDepth.AfterDirective,
        int depthPriority = 10,
        int? timelineDepth = null,
        string? tagName = null,
        int tokenCount = 0,
        string? addressingTerm = null,
        VKInteractionTone? interactionTone = null,
        VKResponseVerbosity? responseVerbosity = null,
        VKEmojiPolicy? emojiPolicy = null)
        => Create(id, displayName, preferredLanguage, description, relativeDepth, depthPriority, timelineDepth, tagName, tokenCount, addressingTerm, interactionTone, responseVerbosity, emojiPolicy);

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKProfilePresence Rehydrate(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? description,
        VKPromptRelativeDepth? relativeDepth,
        int depthPriority,
        int? timelineDepth,
        string? tagName,
        int tokenCount,
        string? addressingTerm = null,
        VKInteractionTone? interactionTone = null,
        VKResponseVerbosity? responseVerbosity = null,
        VKEmojiPolicy? emojiPolicy = null)
    {
        return new VKProfilePresence(
            id,
            displayName,
            preferredLanguage,
            description,
            addressingTerm,
            interactionTone,
            responseVerbosity,
            emojiPolicy,
            relativeDepth,
            depthPriority,
            timelineDepth,
            tagName,
            tokenCount);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the user's display identity and preferred language settings.
    /// </summary>
    public VKResult UpdateSettings(string? displayName, string? preferredLanguage)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        return VKResult.Success();
    }

    /// <summary>
    /// Backward-compatibility overload with deprecated timeZone argument.
    /// </summary>
    [Obsolete("TimeZone has moved to AI.Cognitive.Presence.")]
    public VKResult UpdateSettings(string? displayName, string? preferredLanguage, string? timeZone)
        => UpdateSettings(displayName, preferredLanguage);

    /// <summary>
    /// Updates the user's interaction style and output preferences.
    /// </summary>
    public VKResult UpdateStylePreferences(
        string? addressingTerm = null,
        VKInteractionTone? interactionTone = null,
        VKResponseVerbosity? responseVerbosity = null,
        VKEmojiPolicy? emojiPolicy = null)
    {
        AddressingTerm = addressingTerm;
        InteractionTone = interactionTone;
        ResponseVerbosity = responseVerbosity;
        EmojiPolicy = emojiPolicy;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the custom user description, background, and prompt instructions.
    /// </summary>
    public VKResult UpdateDescription(string? description)
    {
        Description = description;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the precalculated token count for this profile presence.
    /// </summary>
    public VKResult UpdateTokenCount(int tokenCount)
    {
        TokenCount = Math.Max(0, tokenCount);
        return VKResult.Success();
    }
}
