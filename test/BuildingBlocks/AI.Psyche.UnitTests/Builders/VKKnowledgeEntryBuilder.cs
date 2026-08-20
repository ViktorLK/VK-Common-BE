using System;
using VK.Blocks.AI.Psyche;
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
    private VKPromptSegment _segment = new() { Role = VKChatRole.System, Content = "Knowledge Content", IsEnabled = true };
    private string _xmlTag = "knowledge";

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

    public VKKnowledgeEntryBuilder WithContent(string content, VKChatRole role = VKChatRole.System)
    {
        _segment = new VKPromptSegment { Role = role, Content = content, IsEnabled = true };
        return this;
    }

    public VKKnowledgeEntryBuilder WithXmlTag(string xmlTag)
    {
        _xmlTag = xmlTag;
        return this;
    }

    protected override VKKnowledgeEntry CreateDefault()
    {
        return new VKKnowledgeEntry
        {
            Id = _id,
            TriggerType = _triggerType,
            Segment = _segment,
            XmlTag = _xmlTag
        };
    }
}
