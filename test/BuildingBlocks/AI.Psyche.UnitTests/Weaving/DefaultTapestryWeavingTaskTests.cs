using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultTapestryWeavingTaskTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WeavesFragmentsAndUserInputIntoMessages()
    {
        // Arrange
        GetMock<IVKTokenCounter>()
            .Setup(t => t.CountTokens(It.IsAny<string>()))
            .Returns(5);

        var options = new VKWeavingOptions();
        var task = new DefaultTapestryWeavingTask(
            GetMockObject<IVKTokenCounter>(),
            options,
            GetMockObject<ILogger<DefaultTapestryWeavingTask>>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("User Question").BuildContext();
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();

        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "System Instruction" },
            RenderOrder = 1
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.ResponseBuilder.Messages.Should().HaveCount(2);
        context.ResponseBuilder.Messages[0].Role.Should().Be(VKChatRole.System);
        context.ResponseBuilder.Messages[0].Content.Should().Be("System Instruction");
        context.ResponseBuilder.Messages[1].Role.Should().Be(VKChatRole.User);
        context.ResponseBuilder.Messages[1].Content.Should().Be("User Question");
    }

    [Fact]
    public async Task ExecuteAsync_WithAbsoluteInjection_InsertsMessageAtDepth()
    {
        // Arrange
        GetMock<IVKTokenCounter>()
            .Setup(t => t.CountTokens(It.IsAny<string>()))
            .Returns(3);

        var options = new VKWeavingOptions();
        var task = new DefaultTapestryWeavingTask(
            GetMockObject<IVKTokenCounter>(),
            options,
            GetMockObject<ILogger<DefaultTapestryWeavingTask>>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("User Msg").BuildContext();
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();

        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Injected Msg", AbsoluteDepth = 1, DepthPriority = 1 }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.ResponseBuilder.Messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllFragmentsFilteredOut_ReturnsEmptyActiveError()
    {
        // Arrange
        var options = new VKWeavingOptions { DisabledTiers = [VKPromptTierType.Directive] };
        var task = new DefaultTapestryWeavingTask(
            GetMockObject<IVKTokenCounter>(),
            options,
            GetMockObject<ILogger<DefaultTapestryWeavingTask>>());

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Disabled" }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKWeavingErrors.EmptyActive);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleSystemFragmentsPresent_ConcatenatesWithSeparators()
    {
        // Arrange
        GetMock<IVKTokenCounter>().Setup(t => t.CountTokens(It.IsAny<string>())).Returns(10);
        var options = new VKWeavingOptions();
        var task = new DefaultTapestryWeavingTask(
            GetMockObject<IVKTokenCounter>(),
            options,
            GetMockObject<ILogger<DefaultTapestryWeavingTask>>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput(string.Empty).BuildContext();
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Part 1" },
            RenderOrder = 1
        });
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Part 2" },
            Separator = "\n---\n",
            RenderOrder = 2
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.ResponseBuilder.Messages.Should().ContainSingle();
        context.ResponseBuilder.Messages[0].Role.Should().Be(VKChatRole.System);
        context.ResponseBuilder.Messages[0].Content.Should().Be("Part 1\n---\nPart 2");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNonSystemFragmentHasFormatter_AppliesFormatting()
    {
        // Arrange
        GetMock<IVKTokenCounter>().Setup(t => t.CountTokens(It.IsAny<string>())).Returns(10);
        var mockFormatter = GetMock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(true);
        mockFormatter.Setup(f => f.Format(It.IsAny<VKPromptFragment>(), It.IsAny<VKPsycheContext>()))
            .Returns(VKResult.Success("<formatted>Formatted User Msg</formatted>"));

        var options = new VKWeavingOptions();
        var task = new DefaultTapestryWeavingTask(
            GetMockObject<IVKTokenCounter>(),
            options,
            GetMockObject<ILogger<DefaultTapestryWeavingTask>>(),
            formatters: [mockFormatter.Object]);

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Raw Content" },
            RenderOrder = 1
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.ResponseBuilder.Messages.Should().ContainSingle(m => m.Content == "<formatted>Formatted User Msg</formatted>");
    }
}
