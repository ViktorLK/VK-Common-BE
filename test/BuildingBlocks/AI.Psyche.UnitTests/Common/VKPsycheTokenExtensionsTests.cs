using System;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="VKPsycheTokenExtensions"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class VKPsycheTokenExtensionsTests : VKUnitTestBase
{
    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void RefreshTokenCount_PersonaWithRendererAndCounter_UpdatesTokenCount()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();
        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(persona))
            .Returns("Persona content");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Persona content", null))
            .Returns(50);

        // Act
        int tokens = persona.RefreshTokenCount(GetMockObject<IVKPersonaRenderer>(), GetMockObject<IVKTokenCounter>());

        // Assert
        tokens.Should().Be(50);
        persona.TokenCount.Should().Be(50);
    }

    [Fact]
    public void RefreshTokenCount_PersonaWithEvaluator_InvokesEvaluator()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();
        GetMock<IVKPsycheTokenEvaluator>()
            .Setup(e => e.EvaluateAndRefresh(persona, "m1"))
            .Returns(77);

        // Act
        int tokens = persona.RefreshTokenCount(GetMockObject<IVKPsycheTokenEvaluator>(), "m1");

        // Assert
        tokens.Should().Be(77);
    }

    [Fact]
    public void RefreshTokenCount_DirectiveWithRendererAndCounter_UpdatesTokenCount()
    {
        // Arrange
        var directive = new VKDirectiveCharterBuilder().Build();
        GetMock<IVKDirectiveRenderer>()
            .Setup(r => r.Render(directive))
            .Returns("Directive content");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Directive content", "gpt-4"))
            .Returns(80);

        // Act
        int tokens = directive.RefreshTokenCount(GetMockObject<IVKDirectiveRenderer>(), GetMockObject<IVKTokenCounter>(), "gpt-4");

        // Assert
        tokens.Should().Be(80);
        directive.TokenCount.Should().Be(80);
    }

    [Fact]
    public void RefreshTokenCount_DirectiveWithEvaluator_InvokesEvaluator()
    {
        // Arrange
        var directive = new VKDirectiveCharterBuilder().Build();
        GetMock<IVKPsycheTokenEvaluator>()
            .Setup(e => e.EvaluateAndRefresh(directive, null))
            .Returns(90);

        // Act
        int tokens = directive.RefreshTokenCount(GetMockObject<IVKPsycheTokenEvaluator>());

        // Assert
        tokens.Should().Be(90);
    }

    [Fact]
    public void RefreshTokenCount_KnowledgeWithCounter_UpdatesTokenCount()
    {
        // Arrange
        var knowledge = new VKKnowledgeEntryBuilder().WithContent("Knowledge txt").Build();
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Knowledge txt", null))
            .Returns(20);

        // Act
        int tokens = knowledge.RefreshTokenCount(GetMockObject<IVKTokenCounter>());

        // Assert
        tokens.Should().Be(20);
        knowledge.TokenCount.Should().Be(20);
    }

    [Fact]
    public void RefreshTokenCount_KnowledgeWithEvaluator_InvokesEvaluator()
    {
        // Arrange
        var knowledge = new VKKnowledgeEntryBuilder().Build();
        GetMock<IVKPsycheTokenEvaluator>()
            .Setup(e => e.EvaluateAndRefresh(knowledge, "model-x"))
            .Returns(33);

        // Act
        int tokens = knowledge.RefreshTokenCount(GetMockObject<IVKPsycheTokenEvaluator>(), "model-x");

        // Assert
        tokens.Should().Be(33);
    }

    [Fact]
    public void RefreshTokenCount_PatternWithCounter_UpdatesTokenCount()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder().WithContent("Pattern txt").Build();
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Pattern txt", null))
            .Returns(15);

        // Act
        int tokens = pattern.RefreshTokenCount(GetMockObject<IVKTokenCounter>());

        // Assert
        tokens.Should().Be(15);
        pattern.TokenCount.Should().Be(15);
    }

    [Fact]
    public void RefreshTokenCount_PatternWithEvaluator_InvokesEvaluator()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder().Build();
        GetMock<IVKPsycheTokenEvaluator>()
            .Setup(e => e.EvaluateAndRefresh(pattern, null))
            .Returns(22);

        // Act
        int tokens = pattern.RefreshTokenCount(GetMockObject<IVKPsycheTokenEvaluator>());

        // Assert
        tokens.Should().Be(22);
    }

    [Fact]
    public void RefreshTokenCount_ProfileWithRendererAndCounter_UpdatesTokenCount()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();
        GetMock<IVKProfileRenderer>()
            .Setup(r => r.Render(profile))
            .Returns("Profile txt");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Profile txt", null))
            .Returns(45);

        // Act
        int tokens = profile.RefreshTokenCount(GetMockObject<IVKProfileRenderer>(), GetMockObject<IVKTokenCounter>());

        // Assert
        tokens.Should().Be(45);
        profile.TokenCount.Should().Be(45);
    }

    [Fact]
    public void RefreshTokenCount_ProfileWithCounterOnly_UsesDefaultRenderer()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens(It.IsAny<string>(), null))
            .Returns(30);

        // Act
        int tokens = profile.RefreshTokenCount(GetMockObject<IVKTokenCounter>());

        // Assert
        tokens.Should().Be(30);
        profile.TokenCount.Should().Be(30);
    }

    [Fact]
    public void RefreshTokenCount_ProfileWithEvaluator_InvokesEvaluator()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();
        GetMock<IVKPsycheTokenEvaluator>()
            .Setup(e => e.EvaluateAndRefresh(profile, "m2"))
            .Returns(55);

        // Act
        int tokens = profile.RefreshTokenCount(GetMockObject<IVKPsycheTokenEvaluator>(), "m2");

        // Assert
        tokens.Should().Be(55);
    }

    [Fact]
    public void RefreshTokenCount_WhenArgumentsAreNull_ThrowsArgumentNullException()
    {
        var persona = new VKPersonaAnchorBuilder().Build();
        var directive = new VKDirectiveCharterBuilder().Build();
        var knowledge = new VKKnowledgeEntryBuilder().Build();
        var pattern = new VKPatternEntryBuilder().Build();
        var profile = new VKProfilePresenceBuilder().Build();

        Assert.Throws<ArgumentNullException>(() => ((VKPersonaAnchor)null!).RefreshTokenCount(GetMockObject<IVKPersonaRenderer>(), GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => persona.RefreshTokenCount((IVKPersonaRenderer)null!, GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => persona.RefreshTokenCount(GetMockObject<IVKPersonaRenderer>(), (IVKTokenCounter)null!));
        Assert.Throws<ArgumentNullException>(() => persona.RefreshTokenCount((IVKPsycheTokenEvaluator)null!));

        Assert.Throws<ArgumentNullException>(() => ((VKDirectiveCharter)null!).RefreshTokenCount(GetMockObject<IVKDirectiveRenderer>(), GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => directive.RefreshTokenCount((IVKDirectiveRenderer)null!, GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => directive.RefreshTokenCount(GetMockObject<IVKDirectiveRenderer>(), (IVKTokenCounter)null!));
        Assert.Throws<ArgumentNullException>(() => directive.RefreshTokenCount((IVKPsycheTokenEvaluator)null!));

        Assert.Throws<ArgumentNullException>(() => ((VKKnowledgeEntry)null!).RefreshTokenCount(GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => knowledge.RefreshTokenCount((IVKTokenCounter)null!));
        Assert.Throws<ArgumentNullException>(() => knowledge.RefreshTokenCount((IVKPsycheTokenEvaluator)null!));

        Assert.Throws<ArgumentNullException>(() => ((VKPatternEntry)null!).RefreshTokenCount(GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => pattern.RefreshTokenCount((IVKTokenCounter)null!));
        Assert.Throws<ArgumentNullException>(() => pattern.RefreshTokenCount((IVKPsycheTokenEvaluator)null!));

        Assert.Throws<ArgumentNullException>(() => ((VKProfilePresence)null!).RefreshTokenCount(GetMockObject<IVKProfileRenderer>(), GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => profile.RefreshTokenCount((IVKProfileRenderer)null!, GetMockObject<IVKTokenCounter>()));
        Assert.Throws<ArgumentNullException>(() => profile.RefreshTokenCount(GetMockObject<IVKProfileRenderer>(), (IVKTokenCounter)null!));
        Assert.Throws<ArgumentNullException>(() => profile.RefreshTokenCount((IVKPsycheTokenEvaluator)null!));
    }
}
