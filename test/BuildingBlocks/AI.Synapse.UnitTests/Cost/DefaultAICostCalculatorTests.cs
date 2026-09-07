using System;
using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Cost.Internal;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Cost;

/// <summary>
/// Unit tests for <see cref="DefaultAICostCalculator"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class DefaultAICostCalculatorTests
{
    [Fact]
    public void CalculateCost_WhenDisabled_ReturnsZero()
    {
        // Arrange
        var options = new VKCostOptions { Enabled = false };
        var calculator = new DefaultAICostCalculator(options);

        // Act
        var cost = calculator.CalculateCost("OpenAI", VKAIModelIds.OpenAI.Gpt4O, 1000, 1000);

        // Assert
        cost.Should().Be(0.0);
    }

    [Theory]
    [InlineData("OpenAI", VKAIModelIds.OpenAI.Gpt4O, 1000, 1000, 0.005 + 0.015)]
    [InlineData("OpenAI", VKAIModelIds.OpenAI.Gpt4OMini, 2000, 1000, (2 * 0.00015) + (1 * 0.0006))]
    [InlineData("Anthropic", VKAIModelIds.Anthropic.Claude35Sonnet, 1000, 500, 0.003 + (0.5 * 0.015))]
    [InlineData("Google", VKAIModelIds.Google.Gemini20Flash, 10000, 10000, (10 * 0.0001) + (10 * 0.0004))]
    public void CalculateCost_WithPreloadedBaselinePricing_CalculatesExpectedCost(
        string provider,
        string modelId,
        long promptTokens,
        long completionTokens,
        double expectedCost)
    {
        // Arrange
        var options = new VKCostOptions { Enabled = true };
        var calculator = new DefaultAICostCalculator(options);

        // Act
        var cost = calculator.CalculateCost(provider, modelId, promptTokens, completionTokens);

        // Assert
        cost.Should().BeApproximately(expectedCost, 0.000001);
    }

    [Fact]
    public void CalculateCost_WithCustomPricingOverride_UsesCustomRates()
    {
        // Arrange
        var customPricing = new VKModelPricing
        {
            Provider = "CustomProvider",
            ModelId = "custom-llm-v1",
            CostPer1KPromptTokens = 0.02,
            CostPer1KCompletionTokens = 0.05
        };

        var options = new VKCostOptions
        {
            Enabled = true,
            CustomPricing = [customPricing]
        };
        var calculator = new DefaultAICostCalculator(options);

        // Act
        var cost = calculator.CalculateCost("CustomProvider", "custom-llm-v1", 2000, 1000);

        // Assert
        // Prompt: (2000/1000)*0.02 = 0.04; Completion: (1000/1000)*0.05 = 0.05 => Total: 0.09
        cost.Should().BeApproximately(0.09, 0.000001);
    }

    [Fact]
    public void CalculateCost_WithUnknownProviderOrModel_FallsBackToGenericEstimate()
    {
        // Arrange
        var options = new VKCostOptions { Enabled = true };
        var calculator = new DefaultAICostCalculator(options);

        // Act (Unknown model: 500 prompt + 500 completion = 1000 total tokens => $0.001 per 1k fallback)
        var cost = calculator.CalculateCost("UnknownProvider", "unknown-model", 500, 500);

        // Assert
        cost.Should().BeApproximately(0.001, 0.000001);
    }
}
