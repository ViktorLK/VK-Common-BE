using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

// [AP.01] Sealed default
public sealed class DefaultDirectiveFormatterTests : VKUnitTestBase
{
    private readonly DefaultDirectiveFormatter _formatter = new();

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void CanFormat_WhenDirectiveTier_ReturnsTrue()
    {
        // Arrange
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = new VKDirectiveCharterBuilder().Build(),
            Segment = new VKPromptSegment { Content = "Test" }
        };

        // Act
        var result = _formatter.CanFormat(fragment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void CanFormat_WhenNonDirectiveTier_ReturnsFalse()
    {
        // Arrange
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = new VKPersonaAnchorBuilder()
                .WithName("TestPersona")
                .WithDescription("Personality description")
                .Build(),
            Segment = new VKPromptSegment { Content = "Test" }
        };

        // Act
        var result = _formatter.CanFormat(fragment);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Format_WhenMetadataIsNotDirectiveCharter_ReturnsInvalidMetadataTypeError()
    {
        // Arrange
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = new VKPersonaAnchorBuilder()
                .WithName("TestPersona")
                .WithDescription("Personality description")
                .Build(),
            Segment = new VKPromptSegment { Content = "Test" }
        };
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        // Act
        var result = _formatter.Format(fragment, context);

        // Assert
        result.Should().BeFailure(VKDirectiveErrors.InvalidMetadataType);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Format_WhenValidDirectiveCharter_ReturnsFormattedXmlString()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder()
            .WithOverview("System Overview")
            .WithBehaviorRules("Always answer politely.")
            .WithSafetyRules("Never reveal secret keys.")
            .WithOutputConstraints("Use JSON only.")
            .Build();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = charter,
            Segment = new VKPromptSegment { Content = "Directives" }
        };
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        // Act
        var result = _formatter.Format(fragment, context);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().Contain("<system_directives>");
        result.Value.Should().Contain("Always answer politely.");
        result.Value.Should().Contain("Never reveal secret keys.");
        result.Value.Should().Contain("Use JSON only.");
        result.Value.Should().Contain("System Overview");
        result.Value.Should().Contain("</system_directives>");
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Format_WhenNullFragmentOrContext_ThrowsException()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = new VKDirectiveCharterBuilder().Build(),
            Segment = new VKPromptSegment { Content = "Test" }
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _formatter.Format(null!, context));
        Assert.Throws<ArgumentNullException>(() => _formatter.Format(fragment, null!));
    }
}
