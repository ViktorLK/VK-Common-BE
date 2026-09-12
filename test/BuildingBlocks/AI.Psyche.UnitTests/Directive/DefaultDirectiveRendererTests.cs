using System;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

// [AP.01] Sealed default
public sealed class DefaultDirectiveRendererTests : VKUnitTestBase
{
    private readonly DefaultDirectiveRenderer _renderer = new();

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Render_WhenDirectiveIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _renderer.Render(null!));
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Render_WhenValidDirectiveCharter_ReturnsJoinedFormattedText()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder()
            .WithBehaviorRules("Always answer politely.")
            .WithSafetyRules("Never reveal secret keys.")
            .WithOutputConstraints("Use JSON only.")
            .WithOverview("System Overview")
            .Build();

        // Act
        var result = _renderer.Render(charter);

        // Assert
        result.Should().Contain("Always answer politely.");
        result.Should().Contain("Never reveal secret keys.");
        result.Should().Contain("Use JSON only.");
        result.Should().Contain("System Overview");
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Render_WhenAllSectionsEmpty_ReturnsEmptyString()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder()
            .WithOverview(null)
            .WithBehaviorRules(null)
            .WithSafetyRules(null)
            .WithOutputConstraints(null)
            .Build();

        // Act
        var result = _renderer.Render(charter);

        // Assert
        result.Should().BeEmpty();
    }
}
