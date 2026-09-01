using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

public sealed class VKDirectiveCharterTests : VKUnitTestBase
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKDirectiveId(Guid.NewGuid());

        // Act
        var result = VKDirectiveCharter.Create(
            id,
            overview: "Overview",
            behaviorRules: "Rules",
            safetyRules: "Safety",
            outputConstraints: "Constraints");

        // Assert
        result.Should().BeSuccess();
        var charter = result.Value!;
        charter.Id.Should().Be(id);
        charter.Overview.Should().Be("Overview");
        charter.BehaviorRules.Should().Be("Rules");
        charter.SafetyRules.Should().Be("Safety");
        charter.OutputConstraints.Should().Be("Constraints");
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKDirectiveCharter.Create(VKDirectiveId.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidState_RestoresAggregate()
    {
        // Arrange
        var id = new VKDirectiveId(Guid.NewGuid());

        // Act
        var charter = VKDirectiveCharter.Rehydrate(
            id,
            "Overview",
            "Rules",
            "Safety",
            "Constraints");

        // Assert
        charter.Id.Should().Be(id);
        charter.Overview.Should().Be("Overview");
        charter.BehaviorRules.Should().Be("Rules");
        charter.SafetyRules.Should().Be("Safety");
        charter.OutputConstraints.Should().Be("Constraints");
    }

    [Fact]
    public void UpdateOverview_WhenCalled_UpdatesOverview()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = charter.UpdateOverview("New Overview");

        // Assert
        result.Should().BeSuccess();
        charter.Overview.Should().Be("New Overview");
    }

    [Fact]
    public void UpdateBehaviorRules_WhenCalled_UpdatesBehaviorRules()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = charter.UpdateBehaviorRules("New Behavior");

        // Assert
        result.Should().BeSuccess();
        charter.BehaviorRules.Should().Be("New Behavior");
    }

    [Fact]
    public void UpdateSafetyRules_WhenCalled_UpdatesSafetyRules()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = charter.UpdateSafetyRules("New Safety");

        // Assert
        result.Should().BeSuccess();
        charter.SafetyRules.Should().Be("New Safety");
    }

    [Fact]
    public void UpdateOutputConstraints_WhenCalled_UpdatesOutputConstraints()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = charter.UpdateOutputConstraints("New Constraints");

        // Assert
        result.Should().BeSuccess();
        charter.OutputConstraints.Should().Be("New Constraints");
    }

    [Fact]
    public void UpdateContent_WhenCalled_UpdatesAllProperties()
    {
        // Arrange
        var charter = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = charter.UpdateContent("O2", "B2", "S2", "C2");

        // Assert
        result.Should().BeSuccess();
        charter.Overview.Should().Be("O2");
        charter.BehaviorRules.Should().Be("B2");
        charter.SafetyRules.Should().Be("S2");
        charter.OutputConstraints.Should().Be("C2");
    }
}
