using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKDirectiveCharter"/> objects in unit tests.
/// </summary>
public sealed class VKDirectiveCharterBuilder : VKTestDataBuilder<VKDirectiveCharter>
{
    private VKDirectiveId _id = new(Guid.NewGuid());
    private string? _overview = "Default Directive Overview";
    private string? _behaviorRules = "Default Behavior Rules";
    private string? _safetyRules = "Default Safety Rules";
    private string? _outputConstraints = "Default Output Constraints";

    public VKDirectiveCharterBuilder WithId(VKDirectiveId id)
    {
        _id = id;
        return this;
    }

    public VKDirectiveCharterBuilder WithOverview(string? overview)
    {
        _overview = overview;
        return this;
    }

    public VKDirectiveCharterBuilder WithBehaviorRules(string? behaviorRules)
    {
        _behaviorRules = behaviorRules;
        return this;
    }

    public VKDirectiveCharterBuilder WithSafetyRules(string? safetyRules)
    {
        _safetyRules = safetyRules;
        return this;
    }

    public VKDirectiveCharterBuilder WithOutputConstraints(string? outputConstraints)
    {
        _outputConstraints = outputConstraints;
        return this;
    }

    protected override VKDirectiveCharter CreateDefault()
    {
        return VKGuard.NotNull(VKDirectiveCharter.Create(
            _id,
            overview: _overview,
            behaviorRules: _behaviorRules,
            safetyRules: _safetyRules,
            outputConstraints: _outputConstraints).Value);
    }
}
