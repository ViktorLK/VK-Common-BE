using System;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="DefaultPsycheTokenEvaluator"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class DefaultPsycheTokenEvaluatorTests : VKUnitTestBase
{
    private readonly DefaultPsycheTokenEvaluator _evaluator;

    public DefaultPsycheTokenEvaluatorTests()
    {
        _evaluator = new DefaultPsycheTokenEvaluator(
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<IVKProfileRenderer>(),
            GetMockObject<IVKTokenCounter>());
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Constructor_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultPsycheTokenEvaluator(
            null!,
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<IVKProfileRenderer>(),
            GetMockObject<IVKTokenCounter>()));

        Assert.Throws<ArgumentNullException>(() => new DefaultPsycheTokenEvaluator(
            GetMockObject<IVKPersonaRenderer>(),
            null!,
            GetMockObject<IVKProfileRenderer>(),
            GetMockObject<IVKTokenCounter>()));

        Assert.Throws<ArgumentNullException>(() => new DefaultPsycheTokenEvaluator(
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<IVKDirectiveRenderer>(),
            null!,
            GetMockObject<IVKTokenCounter>()));

        Assert.Throws<ArgumentNullException>(() => new DefaultPsycheTokenEvaluator(
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<IVKProfileRenderer>(),
            null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPersonaNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.EvaluateAndRefresh((VKPersonaAnchor)null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPersonaHasContent_CalculatesTokensAndUpdatesAnchor()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();
        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(persona))
            .Returns("Rendered Persona");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Rendered Persona", null))
            .Returns(42);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(persona);

        // Assert
        tokens.Should().Be(42);
        persona.TokenCount.Should().Be(42);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPersonaRenderIsEmpty_SetsZeroTokens()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();
        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(persona))
            .Returns(string.Empty);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(persona);

        // Assert
        tokens.Should().Be(0);
        persona.TokenCount.Should().Be(0);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenDirectiveNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.EvaluateAndRefresh((VKDirectiveCharter)null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenDirectiveHasContent_CalculatesTokensAndUpdatesCharter()
    {
        // Arrange
        var directive = new VKDirectiveCharterBuilder().Build();
        GetMock<IVKDirectiveRenderer>()
            .Setup(r => r.Render(directive))
            .Returns("Rendered Directive");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Rendered Directive", "gpt-4"))
            .Returns(100);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(directive, "gpt-4");

        // Assert
        tokens.Should().Be(100);
        directive.TokenCount.Should().Be(100);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenDirectiveRenderIsEmpty_SetsZeroTokens()
    {
        // Arrange
        var directive = new VKDirectiveCharterBuilder().Build();
        GetMock<IVKDirectiveRenderer>()
            .Setup(r => r.Render(directive))
            .Returns("   ");

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(directive);

        // Assert
        tokens.Should().Be(0);
        directive.TokenCount.Should().Be(0);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenKnowledgeNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.EvaluateAndRefresh((VKKnowledgeEntry)null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenKnowledgeHasContent_CalculatesTokensAndUpdatesEntry()
    {
        // Arrange
        var knowledge = new VKKnowledgeEntryBuilder()
            .WithContent("Knowledge Content")
            .Build();
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Knowledge Content", null))
            .Returns(35);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(knowledge);

        // Assert
        tokens.Should().Be(35);
        knowledge.TokenCount.Should().Be(35);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenKnowledgeSegmentIsEmpty_SetsZeroTokens()
    {
        // Arrange
        var knowledge = new VKKnowledgeEntryBuilder()
            .WithContent(string.Empty)
            .Build();

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(knowledge);

        // Assert
        tokens.Should().Be(0);
        knowledge.TokenCount.Should().Be(0);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPatternNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.EvaluateAndRefresh((VKPatternEntry)null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPatternHasContent_CalculatesTokensAndUpdatesEntry()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder()
            .WithContent("Pattern Content")
            .Build();
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Pattern Content", null))
            .Returns(18);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(pattern);

        // Assert
        tokens.Should().Be(18);
        pattern.TokenCount.Should().Be(18);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenPatternSegmentIsEmpty_SetsZeroTokens()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder()
            .WithContent("   ")
            .Build();

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(pattern);

        // Assert
        tokens.Should().Be(0);
        pattern.TokenCount.Should().Be(0);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenProfileNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.EvaluateAndRefresh((VKProfilePresence)null!));
    }

    [Fact]
    public void EvaluateAndRefresh_WhenProfileHasContent_CalculatesTokensAndUpdatesPresence()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();
        GetMock<IVKProfileRenderer>()
            .Setup(r => r.Render(profile))
            .Returns("Rendered Profile");
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Rendered Profile", null))
            .Returns(60);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(profile);

        // Assert
        tokens.Should().Be(60);
        profile.TokenCount.Should().Be(60);
    }

    [Fact]
    public void EvaluateAndRefresh_WhenProfileRenderIsEmpty_SetsZeroTokens()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();
        GetMock<IVKProfileRenderer>()
            .Setup(r => r.Render(profile))
            .Returns(string.Empty);

        // Act
        int tokens = _evaluator.EvaluateAndRefresh(profile);

        // Assert
        tokens.Should().Be(0);
        profile.TokenCount.Should().Be(0);
    }

    [Fact]
    public void Evaluate_WhenSegmentNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _evaluator.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_WhenSegmentHasContent_ReturnsNewSegmentWithTokenCount()
    {
        // Arrange
        var segment = new VKPromptSegment { Content = "Segment Text", TokenCount = 0 };
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens("Segment Text", "custom-model"))
            .Returns(25);

        // Act
        var evaluated = _evaluator.Evaluate(segment, "custom-model");

        // Assert
        evaluated.TokenCount.Should().Be(25);
        evaluated.Content.Should().Be("Segment Text");
    }

    [Fact]
    public void Evaluate_WhenSegmentContentIsEmpty_ReturnsSegmentWithZeroTokens()
    {
        // Arrange
        var segmentWithTokens = new VKPromptSegment { Content = "   ", TokenCount = 10 };
        var segmentWithoutTokens = new VKPromptSegment { Content = null!, TokenCount = 0 };

        // Act
        var res1 = _evaluator.Evaluate(segmentWithTokens);
        var res2 = _evaluator.Evaluate(segmentWithoutTokens);

        // Assert
        res1.TokenCount.Should().Be(0);
        res2.TokenCount.Should().Be(0);
    }
}
