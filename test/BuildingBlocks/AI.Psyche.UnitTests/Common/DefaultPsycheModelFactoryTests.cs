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
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);

        // Act
        var persona = factory.CreatePersona("Assistant", "Helper");

        // Assert
        persona.Id.Value.Should().Be(newGuid);
        persona.TenantId.Should().Be(VKTenantId.Default);
        persona.Name.Should().Be("Assistant");
        persona.Description.Should().Be("Helper");
    }

    [Fact]
    public void CreateDirective_CreatesValidDirectiveCharter()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);

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
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);
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
        var identityMock = new Mock<IVKIdentityContext>();
        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);
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
        var userId = new VKUserId(Guid.NewGuid());
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        identityMock.SetupGet(i => i.UserId).Returns(userId);

        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);
        var personaId = new VKPersonaId(Guid.NewGuid());

        // Act
        var session = factory.CreateSession(personaId);

        // Assert
        session.Id.Value.Should().Be(newGuid);
        session.PersonaId.Should().Be(personaId);
        session.UserId.Should().Be(userId);
        session.Status.Should().Be(VKSessionStatus.Active);
    }

    [Fact]
    public void CreateEcho_CreatesValidEchoTrace()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var guidGenMock = new Mock<IVKGuidGenerator>();
        var newGuid = Guid.NewGuid();
        guidGenMock.Setup(g => g.Create()).Returns(newGuid);

        var factory = new DefaultPsycheModelFactory(identityMock.Object, guidGenMock.Object, TimeProvider.System);
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
