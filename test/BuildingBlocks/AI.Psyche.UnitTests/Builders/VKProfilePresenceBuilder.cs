using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKProfilePresence"/> objects in unit tests.
/// </summary>
public sealed class VKProfilePresenceBuilder : VKTestDataBuilder<VKProfilePresence>
{
    private VKProfileId _id = new(Guid.NewGuid());
    private string? _displayName = "Default User";
    private string? _preferredLanguage = "en-US";
    private string? _timeZone = "UTC";
    private Dictionary<string, string> _preferences = new();

    public VKProfilePresenceBuilder WithId(VKProfileId id)
    {
        _id = id;
        return this;
    }

    public VKProfilePresenceBuilder WithDisplayName(string? displayName)
    {
        _displayName = displayName;
        return this;
    }

    public VKProfilePresenceBuilder WithPreferredLanguage(string? preferredLanguage)
    {
        _preferredLanguage = preferredLanguage;
        return this;
    }

    public VKProfilePresenceBuilder WithTimeZone(string? timeZone)
    {
        _timeZone = timeZone;
        return this;
    }

    public VKProfilePresenceBuilder WithPreference(string key, string value)
    {
        _preferences[key] = value;
        return this;
    }

    public VKProfilePresenceBuilder WithPreferences(IReadOnlyDictionary<string, string> preferences)
    {
        _preferences = new Dictionary<string, string>(preferences);
        return this;
    }

    protected override VKProfilePresence CreateDefault()
    {
        return VKGuard.NotNull(VKProfilePresence.Create(
            _id,
            displayName: _displayName,
            preferredLanguage: _preferredLanguage,
            timeZone: _timeZone,
            preferences: _preferences).Value);
    }
}
