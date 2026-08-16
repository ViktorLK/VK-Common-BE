using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for the <see cref="DefaultKnowledgeStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultKnowledgeStageTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(
        string personaId = "test-persona",
        string userInput = "test input")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaId = new VKPersonaId(System.Guid.NewGuid()),
            SessionId = new VKSessionId(System.Guid.NewGuid()),
            UserInput = userInput
        };

        var context = new VKPsycheContext
        {
            Request = request,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeywordMatches_AddsKnowledgeFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var options = new VKKnowledgeOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var entryId = new VKKnowledgeId(System.Guid.NewGuid());
        var entry = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = entryId,
            TriggerType = VKKnowledgeTriggerType.Keyword,
            Segment = new VKPromptSegment
            {
                Role = VKChatRole.System,
                Content = "Apples are delicious fruits.",
                IsEnabled = true
            },
            Keys = new List<VKKnowledgeKey>
            {
                new() { Text = "apple", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false }
            }
        };

        IEnumerable<VKKnowledgeEntry> entries = [entry];
        storeMock.Setup(s => s.GetRelevantEntriesAsync(It.IsAny<VKPersonaId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(entries));

        var stage = new DefaultKnowledgeStage(options, storeMock.Object, weavingOptions);
        var finalizer = new DefaultKnowledgeFinalizerStage();
        var (context, _) = CreateTestContext(userInput: "I really like to eat an apple every day!");

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge);

        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        fragment.Metadata.Should().BeOfType<VKKnowledgeEntry>();

        var parsedEntry = (VKKnowledgeEntry)fragment.Metadata!;
        parsedEntry.Id.Should().Be(entryId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConstant_AddsKnowledgeFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var options = new VKKnowledgeOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var entryId = new VKKnowledgeId(System.Guid.NewGuid());
        var entry = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = entryId,
            TriggerType = VKKnowledgeTriggerType.Constant,
            Segment = new VKPromptSegment
            {
                Role = VKChatRole.System,
                Content = "Constant lore.",
                IsEnabled = true
            }
        };

        storeMock.Setup(s => s.GetRelevantEntriesAsync(It.IsAny<VKPersonaId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKKnowledgeEntry>>([entry]));

        var stage = new DefaultKnowledgeStage(options, storeMock.Object, weavingOptions);
        var finalizer = new DefaultKnowledgeFinalizerStage();
        var (context, _) = CreateTestContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        var parsedEntry = (VKKnowledgeEntry)fragment.Metadata!;
        parsedEntry.Id.Should().Be(entryId);
    }
}
