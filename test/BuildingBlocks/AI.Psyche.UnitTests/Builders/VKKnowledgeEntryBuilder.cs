using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKKnowledgeEntry"/> objects in unit tests.
/// </summary>
public sealed class VKKnowledgeEntryBuilder : VKTestDataBuilder<VKKnowledgeEntry>
{
    private VKKnowledgeId _id = new(Guid.NewGuid());
    private VKKnowledgeTriggerType _triggerType = VKKnowledgeTriggerType.Constant;
    private VKKnowledgeFilterLogic _filterLogic = VKKnowledgeFilterLogic.AndAny;
    private VKPromptSegment _segment = new() { Role = VKChatRole.System, Content = "Knowledge Content" };
    private string? _xmlTag;
    private List<VKKnowledgeKey> _keys = [];

    public VKKnowledgeEntryBuilder WithId(VKKnowledgeId id)
    {
        _id = id;
        return this;
    }

    public VKKnowledgeEntryBuilder WithTriggerType(VKKnowledgeTriggerType triggerType)
    {
        _triggerType = triggerType;
        return this;
    }

    public VKKnowledgeEntryBuilder WithFilterLogic(VKKnowledgeFilterLogic filterLogic)
    {
        _filterLogic = filterLogic;
        return this;
    }

    public VKKnowledgeEntryBuilder WithContent(string content, VKChatRole role = VKChatRole.System)
    {
        _segment = new VKPromptSegment { Role = role, Content = content };
        return this;
    }

    public VKKnowledgeEntryBuilder WithSegment(VKPromptSegment segment)
    {
        _segment = segment;
        return this;
    }

    public VKKnowledgeEntryBuilder WithXmlTag(string? xmlTag)
    {
        _xmlTag = xmlTag;
        return this;
    }

    public VKKnowledgeEntryBuilder WithKey(VKKnowledgeKey key)
    {
        _keys.Add(key);
        return this;
    }

    public VKKnowledgeEntryBuilder WithKeys(IEnumerable<VKKnowledgeKey> keys)
    {
        _keys.AddRange(keys);
        return this;
    }

    protected override VKKnowledgeEntry CreateDefault()
    {
        var segment = _xmlTag is not null ? _segment with { TagName = _xmlTag } : _segment;
        return VKGuard.NotNull(VKKnowledgeEntry.Create(
            _id,
            segment,
            _triggerType,
            _filterLogic,
            _keys.Count > 0 ? _keys : null).Value);
    }
}
