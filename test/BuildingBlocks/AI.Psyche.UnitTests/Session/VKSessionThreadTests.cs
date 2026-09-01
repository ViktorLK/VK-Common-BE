using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

/// <summary>
/// Unit tests for <see cref="VKSessionThread"/> aggregate root.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKSessionThreadTests : VKUnitTestBase
{
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKSessionId(Guid.NewGuid());

        // Act
        var result = VKSessionThread.Create(id, _now, VKSessionMode.Continuous);

        // Assert
        result.Should().BeSuccess();
        var thread = result.Value!;
        thread.Id.Should().Be(id);
        thread.Mode.Should().Be(VKSessionMode.Continuous);
        thread.Status.Should().Be(VKSessionStatus.Active);
        thread.TurnCount.Should().Be(0);
        thread.CreatedAt.Should().Be(_now);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKSessionThread.Create(VKSessionId.Empty, _now);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidParameters_RestoresAggregate()
    {
        // Arrange
        var id = new VKSessionId(Guid.NewGuid());
        var state = new VKSessionKnowledgeState();

        // Act
        var thread = VKSessionThread.Rehydrate(
            id,
            VKSessionMode.Sandbox,
            parentSessionId: null,
            forkSourceSessionId: null,
            forkPointRef: null,
            status: VKSessionStatus.Active,
            turnCount: 5,
            knowledgeState: state,
            createdAt: _now.AddDays(-1),
            updatedAt: _now,
            lastActivityAt: _now);

        // Assert
        thread.Id.Should().Be(id);
        thread.TurnCount.Should().Be(5);
    }

    [Fact]
    public void IncrementTurn_WhenActive_IncrementsTurnCount()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();

        // Act
        var result = thread.IncrementTurn(_now.AddMinutes(1));

        // Assert
        result.Should().BeSuccess();
        thread.TurnCount.Should().Be(1);
        thread.LastActivityAt.Should().Be(_now.AddMinutes(1));
    }

    [Fact]
    public void IncrementTurn_WhenNotActive_ReturnsFailure()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        thread.Close(_now);

        // Act
        var result = thread.IncrementTurn(_now.AddMinutes(1));

        // Assert
        result.Should().BeFailure(VKSessionErrors.SessionNotActive);
    }

    [Fact]
    public void AdvanceKnowledgeState_WhenActive_UpdatesState()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        var newState = new VKSessionKnowledgeState { LastEvaluatedTurn = 3 };

        // Act
        var result = thread.AdvanceKnowledgeState(newState, _now.AddMinutes(2));

        // Assert
        result.Should().BeSuccess();
        thread.KnowledgeState.Should().BeSameAs(newState);
    }

    [Fact]
    public void AdvanceKnowledgeState_WhenNotActive_ReturnsFailure()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        thread.Close(_now);

        // Act
        var result = thread.AdvanceKnowledgeState(new VKSessionKnowledgeState(), _now);

        // Assert
        result.Should().BeFailure(VKSessionErrors.SessionNotActive);
    }

    [Fact]
    public void ChangeStatus_WhenClosedToActive_ReturnsFailure()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        thread.Close(_now);

        // Act
        var result = thread.ChangeStatus(VKSessionStatus.Active, _now);

        // Assert
        result.Should().BeFailure(VKSessionErrors.SessionNotActive);
    }

    [Fact]
    public void Fork_WithValidParameters_CreatesForkedChild()
    {
        // Arrange
        var thread = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        var childId = new VKSessionThreadBuilder().Build().Id;

        // Act
        var result = thread.Fork(childId, "checkpoint-1", _now.AddMinutes(5));

        // Assert
        result.Should().BeSuccess();
        var child = result.Value!;
        child.Id.Should().Be(childId);
        child.ForkSourceSessionId.Should().Be(thread.Id);
        child.ForkPointRef.Should().Be("checkpoint-1");
    }
}
