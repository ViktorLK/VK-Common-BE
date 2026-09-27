using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="VKPsycheTokenBudget"/>.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKPsycheTokenBudgetTests : VKUnitTestBase
{
    [Fact]
    public void TokenBudget_AvailablePromptBudget_WhenLimitHigherThanReserved_ReturnsDifference()
    {
        // Arrange
        var budget = new VKPsycheTokenBudget
        {
            TotalLimit = 10000,
            ReservedResponseTokens = 2000
        };

        // Assert
        budget.HasLimit.Should().BeTrue();
        budget.AvailablePromptBudget.Should().Be(8000);
    }

    [Fact]
    public void TokenBudget_AvailablePromptBudget_WhenReservedExceedsLimit_ReturnsZero()
    {
        // Arrange
        var budget = new VKPsycheTokenBudget
        {
            TotalLimit = 1000,
            ReservedResponseTokens = 2000
        };

        // Assert
        budget.HasLimit.Should().BeTrue();
        budget.AvailablePromptBudget.Should().Be(0);
    }

    [Fact]
    public void TokenBudget_WhenTotalLimitNull_AvailablePromptBudgetReturnsIntMaxValue()
    {
        // Arrange
        var budget = new VKPsycheTokenBudget
        {
            TotalLimit = null,
            ReservedResponseTokens = 4000
        };

        // Assert
        budget.HasLimit.Should().BeFalse();
        budget.AvailablePromptBudget.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Context_TokenBudget_RoundtripAssignment()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        var budget = new VKPsycheTokenBudget
        {
            TotalLimit = 64000,
            ReservedResponseTokens = 4000
        };

        // Act
        context.TokenBudget = budget;

        // Assert
        context.TokenBudget.Should().NotBeNull();
        context.TokenBudget!.TotalLimit.Should().Be(64000);
        context.TokenBudget.AvailablePromptBudget.Should().Be(60000);
    }
}
