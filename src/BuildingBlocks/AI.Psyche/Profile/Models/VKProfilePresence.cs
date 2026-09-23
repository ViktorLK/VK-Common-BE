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
    /// Gets the layout coordinates and placement rules for this profile presence.
    /// </summary>
    public VKPromptCoordinates Coordinates { get; private set; }

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
        VKPromptCoordinates coordinates,
        int tokenCount) : base(id)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        Description = description;
        AddressingTerm = addressingTerm;
        InteractionTone = interactionTone;
        ResponseVerbosity = responseVerbosity;
        EmojiPolicy = emojiPolicy;
        Coordinates = coordinates;
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
        VKPromptCoordinates? coordinates = null,
        int tokenCount = 0,
        string? addressingTerm = null,
        VKInteractionTone? interactionTone = null,
        VKResponseVerbosity? responseVerbosity = null,
        VKEmojiPolicy? emojiPolicy = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        return VKResult.Success(new VKProfilePresence(
            id,
            displayName,
            preferredLanguage,
            description,
            addressingTerm,
            interactionTone,
            responseVerbosity,
            emojiPolicy,
            coordinates ?? VKPromptCoordinates.Default,
            Math.Max(0, tokenCount)));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKProfilePresence Rehydrate(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? description,
        VKPromptCoordinates coordinates,
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
            coordinates,
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
    /// Updates the prompt layout coordinates for this profile presence.
    /// </summary>
    public VKResult UpdateCoordinates(VKPromptCoordinates coordinates)
    {
        Coordinates = VKGuard.NotNull(coordinates);
        return VKResult.Success();
    }

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
