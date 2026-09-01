using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKPatternEntry"/> objects in unit tests.
/// </summary>
public sealed class VKPatternEntryBuilder : VKTestDataBuilder<VKPatternEntry>
{
    private VKPatternId _id = new(Guid.NewGuid());
    private VKPromptSegment _segment = new()
    {
        Content = "Default Pattern Segment Content",
        IsEnabled = true
    };

    public VKPatternEntryBuilder WithId(VKPatternId id)
    {
        _id = id;
        return this;
    }

    public VKPatternEntryBuilder WithContent(string content)
    {
        _segment = new VKPromptSegment
        {
            Content = content,
            IsEnabled = true
        };
        return this;
    }

    public VKPatternEntryBuilder WithSegment(VKPromptSegment segment)
    {
        _segment = segment;
        return this;
    }

    protected override VKPatternEntry CreateDefault()
    {
        return VKGuard.NotNull(VKPatternEntry.Create(_id, _segment).Value);
    }
}
