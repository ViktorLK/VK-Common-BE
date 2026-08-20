using System;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

public sealed class DefaultPsycheModelFactoryTests
{
    [Fact]
    public void CreatePersona_CreatesValidPersonaAnchor()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);

        // Act
        var persona = factory.CreatePersona("Assistant", "Helper");

        // Assert
        persona.Id.Value.Should().Be(newGuid);
        persona.Name.Should().Be("Assistant");
        persona.Description.Should().Be("Helper");
    }

    [Fact]
    public void CreateDirective_CreatesValidDirectiveCharter()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);

        // Act
        var directive = factory.CreateDirective("Overview", "Rules", "Safety", "Constraints");

        // Assert
        directive.Id.Value.Should().Be(newGuid);
        directive.Overview.Should().Be("Overview");
        directive.BehaviorRules.Should().Be("Rules");
    }

    [Fact]
    public void CreateKnowledge_CreatesValidKnowledgeEntry()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);
        var segment = new VKPromptSegment { Content = "Knowledge Item" };

        // Act
        var entry = factory.CreateKnowledge(segment);

        // Assert
        entry.Id.Value.Should().Be(newGuid);
        entry.Segment.Content.Should().Be("Knowledge Item");
    }

    [Fact]
    public void CreatePattern_CreatesValidPatternEntry()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);
        var segment = new VKPromptSegment { Content = "Pattern Item" };

        // Act
        var entry = factory.CreatePattern(segment);

        // Assert
        entry.Id.Value.Should().Be(newGuid);
        entry.Segment.Content.Should().Be("Pattern Item");
    }

    [Fact]
    public void CreateSession_CreatesValidSessionThread()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);
        var personaId = new VKPersonaId(Guid.NewGuid());

        // Act
        var session = factory.CreateSession(VKSessionMode.Isolated);

        // Assert
        session.Id.Value.Should().Be(newGuid);
        session.Mode.Should().Be(VKSessionMode.Isolated);
        session.Status.Should().Be(VKSessionStatus.Active);
    }

    [Fact]
    public void CreateProfile_CreatesValidProfilePresence()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);

        // Act
        var profile = factory.CreateProfile("Hero", "zh-CN", "UTC");

        // Assert
        profile.Id.Value.Should().Be(newGuid);
        profile.DisplayName.Should().Be("Hero");
        profile.PreferredLanguage.Should().Be("zh-CN");
        profile.TimeZone.Should().Be("UTC");
    }

    [Fact]
    public void CreateEcho_CreatesValidEchoTrace()
    {
        // Arrange
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(guidGenMock.Object, TimeProvider.System);
        var sessionId = new VKSessionId(Guid.NewGuid());

        // Act
        var trace = factory.CreateEcho(sessionId, VKChatRole.User, "User input message");

        // Assert
        trace.Id.Value.Should().Be(newGuid);
        trace.SessionId.Should().Be(sessionId);
        trace.Content.Should().Be("User input message");
        trace.Role.Should().Be(VKChatRole.User);
    }
}
