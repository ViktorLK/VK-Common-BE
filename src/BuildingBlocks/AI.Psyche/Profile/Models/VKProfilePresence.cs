using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a user's lightweight cognitive presence in Psyche's execution pipeline.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKProfilePresence : VKAggregateRoot<VKProfileId>, IVKFragmentMetadata
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the user preferred display name.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the user preferred language code (e.g. "en-US", "ja-JP").
    /// </summary>
    public string? PreferredLanguage { get; private set; }

    /// <summary>
    /// Gets the user standard IANA or Windows time zone.
    /// </summary>
    public string? TimeZone { get; private set; }

    /// <summary>
    /// Gets arbitrary key-value user preference settings for prompt personalizations.
    /// </summary>
    public IReadOnlyDictionary<string, string> Preferences { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKProfilePresence(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? timeZone,
        IReadOnlyDictionary<string, string>? preferences) : base(id)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        TimeZone = timeZone;
        Preferences = preferences ?? new Dictionary<string, string>();
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
        string? timeZone = null,
        IReadOnlyDictionary<string, string>? preferences = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        return VKResult.Success(new VKProfilePresence(id, displayName, preferredLanguage, timeZone, preferences));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKProfilePresence Rehydrate(
        VKProfileId id,
        string? displayName,
        string? preferredLanguage,
        string? timeZone,
        IReadOnlyDictionary<string, string>? preferences = null)
    {
        return new VKProfilePresence(id, displayName, preferredLanguage, timeZone, preferences);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the user's display identity, preferred language, and timezone settings.
    /// </summary>
    public VKResult UpdateSettings(string? displayName, string? preferredLanguage, string? timeZone)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        TimeZone = timeZone;
        return VKResult.Success();
    }

    /// <summary>
    /// Sets or updates a single user preference key-value pair.
    /// </summary>
    public VKResult SetPreference(string key, string value)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        var dict = new Dictionary<string, string>(Preferences) { [key] = value };
        Preferences = dict;
        return VKResult.Success();
    }

    /// <summary>
    /// Removes a user preference key if present.
    /// </summary>
    public VKResult RemovePreference(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (!Preferences.ContainsKey(key))
        {
            return VKResult.Success();
        }

        var dict = new Dictionary<string, string>(Preferences);
        dict.Remove(key);
        Preferences = dict;
        return VKResult.Success();
    }
}
