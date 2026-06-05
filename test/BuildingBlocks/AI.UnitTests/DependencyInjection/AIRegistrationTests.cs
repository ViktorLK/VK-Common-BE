using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;
using FluentAssertions;
using Xunit;

namespace VK.Blocks.AI.UnitTests.DependencyInjection;

// // [AP.01] Sealed default for test classes
public sealed class AIRegistrationTests
{
    [Fact]
    // // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIBlock_WithNestedConfig_ShouldBindSubFeatureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register core block prerequisite [AP.02]
        services.AddVKBlockMarker<VKCoreBlock>();

        var configData = new Dictionary<string, string?>
        {
            ["VKBlocks:AI:Enabled"] = "true",
            ["VKBlocks:AI:Chat:Enabled"] = "true",
            ["VKBlocks:AI:Chat:ModelId"] = "gpt-4-turbo-test"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddVKAIBlock(configuration)
                .AddVKChat();

        using var serviceProvider = services.BuildServiceProvider();
        var chatOptions = serviceProvider.GetRequiredService<IOptions<VKChatOptions>>().Value;

        // Assert
        chatOptions.ModelId.Should().Be("gpt-4-turbo-test");
    }

    [Fact]
    // // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIBlock_WithTokenicsBudgeting_ShouldBindBudgetingOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register core block prerequisite [AP.02]
        services.AddVKBlockMarker<VKCoreBlock>();

        var configData = new Dictionary<string, string?>
        {
            ["VKBlocks:AI:Enabled"] = "true",
            ["VKBlocks:AI:Tokenics:Budgeting:Enabled"] = "true",
            ["VKBlocks:AI:Tokenics:Budgeting:SafetyMargin"] = "999"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddVKAIBlock(configuration)
                .AddVKTokenics()
                .AddVKBudgeting();

        using var serviceProvider = services.BuildServiceProvider();
        var budgetingOptions = serviceProvider.GetRequiredService<IOptions<VKBudgetingOptions>>().Value;

        // Assert
        budgetingOptions.SafetyMargin.Should().Be(999);
    }
}
