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
    private string? _description = null;
    private VKPromptRelativeDepth? _relativeDepth = VKPromptRelativeDepth.AfterDirective;
    private int _depthPriority = 10;
    private int? _absoluteDepth = null;
    private string? _tagName = null;
    private int _tokenCount = 0;

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

    public VKProfilePresenceBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public VKProfilePresenceBuilder WithContent(string content)
    {
        _description = content;
        return this;
    }

    public VKProfilePresenceBuilder WithCoordinates(
        VKPromptRelativeDepth? relativeDepth,
        int depthPriority = 10,
        int? absoluteDepth = null,
        string? tagName = null)
    {
        _relativeDepth = relativeDepth;
        _depthPriority = depthPriority;
        _absoluteDepth = absoluteDepth;
        _tagName = tagName;
        return this;
    }

    public VKProfilePresenceBuilder WithTokenCount(int tokenCount)
    {
        _tokenCount = tokenCount;
        return this;
    }

    protected override VKProfilePresence CreateDefault()
    {
        return VKGuard.NotNull(VKProfilePresence.Create(
            _id,
            displayName: _displayName,
            preferredLanguage: _preferredLanguage,
            timeZone: _timeZone,
            description: _description,
            relativeDepth: _relativeDepth,
            depthPriority: _depthPriority,
            absoluteDepth: _absoluteDepth,
            tagName: _tagName,
            tokenCount: _tokenCount).Value);
    }
}
