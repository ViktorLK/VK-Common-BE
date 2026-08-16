using System;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class DefaultEchoFormatterTests
{
    [Fact]
    public void CanFormat_ReturnsTrue_OnlyForEchoTier()
    {
        // Arrange
        var rendererMock = new Mock<IVKEchoRenderer>();
        var formatter = new DefaultEchoFormatter(rendererMock.Object);
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;

        // Act & Assert
        formatter.CanFormat(new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment()
        }).Should().BeTrue();

        formatter.CanFormat(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment()
        }).Should().BeFalse();
    }

    [Fact]
    public void Format_WithValidTrace_RendersAndReturnsSuccess()
    {
        // Arrange
        var rendererMock = new Mock<IVKEchoRenderer>();
        rendererMock.Setup(r => r.Render(It.IsAny<VKEchoTrace>(), It.IsAny<VKPsycheContext>()))
            .Returns("[User]: Hello");

        var formatter = new DefaultEchoFormatter(rendererMock.Object);
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Hello"
        };
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Metadata = trace,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = formatter.Format(fragment, context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("[User]: Hello");
    }

    [Fact]
    public void Format_WithInvalidMetadata_ReturnsFailure()
    {
        // Arrange
        var rendererMock = new Mock<IVKEchoRenderer>();
        var formatter = new DefaultEchoFormatter(rendererMock.Object);
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = formatter.Format(fragment, context);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
