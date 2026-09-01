using System.Diagnostics;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="VKPsycheSemanticExtensions"/>.
/// Follows AP.01 and DL.01.
/// </summary>
public sealed class VKPsycheSemanticExtensionsTests : VKUnitTestBase
{
    [Fact]
    public void SetPsycheTags_WhenActivityIsNull_DoesNotThrow()
    {
        // Arrange
        Activity? activity = null;

        // Act & Assert
        activity.SetPsycheStage("Stage").Should().BeNull();
        activity.SetPsycheCorrelationId("Corr").Should().BeNull();
        activity.SetPsycheKnowledgeCount(5).Should().BeNull();
        activity.SetPsycheEchoCount(2, 1).Should().BeNull();
        activity.SetPsycheMessageCount(10).Should().BeNull();
    }

    [Fact]
    public void SetPsycheTags_WhenActivityIsNotNull_SetsTagsCorrectly()
    {
        // Arrange
        using var activity = new Activity("TestActivity");
        activity.Start();

        // Act
        activity.SetPsycheStage("DirectiveStage");
        activity.SetPsycheCorrelationId("corr-123");
        activity.SetPsycheKnowledgeCount(3);
        activity.SetPsycheEchoCount(4, 2);
        activity.SetPsycheMessageCount(7);

        // Assert
        activity.GetTagItem(VKPsycheSemanticExtensions.StageKey).Should().Be("DirectiveStage");
        activity.GetTagItem(VKPsycheSemanticExtensions.CorrelationIdKey).Should().Be("corr-123");
        activity.GetTagItem(VKPsycheSemanticExtensions.MatchedCountKey).Should().Be(3);
        activity.GetTagItem(VKPsycheSemanticExtensions.RetainedEchoKey).Should().Be(4);
        activity.GetTagItem(VKPsycheSemanticExtensions.TrimmedEchoKey).Should().Be(2);
        activity.GetTagItem(VKPsycheSemanticExtensions.MessageCountKey).Should().Be(7);

        activity.Stop();
    }
}
