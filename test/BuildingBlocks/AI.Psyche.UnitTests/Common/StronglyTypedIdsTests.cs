using System;
using FluentAssertions;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

public sealed class StronglyTypedIdsTests
{
    [Fact]
    public void VKDirectiveId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKDirectiveId(guid);
        var id2 = new VKDirectiveId(guid);
        var id3 = new VKDirectiveId(Guid.NewGuid());

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        (id1 != id3).Should().BeTrue();
        id1.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void VKEchoId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKEchoId(guid);
        var id2 = new VKEchoId(guid);

        id1.Should().Be(id2);
        id1.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void VKKnowledgeId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKKnowledgeId(guid);
        var id2 = new VKKnowledgeId(guid);

        id1.Should().Be(id2);
        id1.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void VKPersonaId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKPersonaId(guid);
        var id2 = new VKPersonaId(guid);

        id1.Should().Be(id2);
        id1.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void VKSessionId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKSessionId(guid);
        var id2 = new VKSessionId(guid);

        id1.Should().Be(id2);
        id1.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void VKPatternId_EqualityAndToString_BehavesCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new VKPatternId(guid);
        var id2 = new VKPatternId(guid);

        id1.Should().Be(id2);
        id1.ToString().Should().Be(guid.ToString());
    }
}
