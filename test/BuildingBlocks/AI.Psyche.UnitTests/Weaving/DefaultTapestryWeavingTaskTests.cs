using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultTapestryWeavingTaskTests
{
    [Fact]
    public async Task ExecuteAsync_WeavesFragmentsAndUserInputIntoMessages()
    {
        // Arrange
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        tokenCounterMock.Setup(t => t.CountTokens(It.IsAny<string>())).Returns(5);

        var options = new VKWeavingOptions();
        var loggerMock = new Mock<ILogger<DefaultTapestryWeavingTask>>();
        var task = new DefaultTapestryWeavingTask(tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("User Question").BuildContext();
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;

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
        result.IsSuccess.Should().BeTrue();
        context.Response.Messages.Should().HaveCount(2);
        context.Response.Messages[0].Role.Should().Be(VKChatRole.System);
        context.Response.Messages[0].Content.Should().Be("System Instruction");
        context.Response.Messages[1].Role.Should().Be(VKChatRole.User);
        context.Response.Messages[1].Content.Should().Be("User Question");
    }

    [Fact]
    public async Task ExecuteAsync_WithAbsoluteInjection_InsertsMessageAtDepth()
    {
        // Arrange
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        tokenCounterMock.Setup(t => t.CountTokens(It.IsAny<string>())).Returns(3);

        var options = new VKWeavingOptions();
        var loggerMock = new Mock<ILogger<DefaultTapestryWeavingTask>>();
        var task = new DefaultTapestryWeavingTask(tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("User Msg").BuildContext();
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;

        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Injected Msg", AbsoluteDepth = 1, DepthPriority = 1 }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Response.Messages.Should().HaveCount(2);
    }
}
