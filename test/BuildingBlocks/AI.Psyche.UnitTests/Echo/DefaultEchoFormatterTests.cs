using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class DefaultEchoFormatterTests : VKUnitTestBase
{
    [Fact]
    public void CanFormat_ReturnsTrue_OnlyForEchoTier()
    {
        // Arrange
        var formatter = new DefaultEchoFormatter(GetMockObject<IVKEchoRenderer>());
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();

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
        GetMock<IVKEchoRenderer>()
            .Setup(r => r.Render(It.IsAny<VKEchoTrace>(), It.IsAny<VKPsycheContext>()))
            .Returns("[User]: Hello");

        var formatter = new DefaultEchoFormatter(GetMockObject<IVKEchoRenderer>());
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("Hello")
            .Build();
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
        result.Should().BeSuccess();
        result.Value.Should().Be("[User]: Hello");
    }

    [Fact]
    public void Format_WithInvalidMetadata_ReturnsFailure()
    {
        // Arrange
        var formatter = new DefaultEchoFormatter(GetMockObject<IVKEchoRenderer>());
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();
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
        result.Should().BeFailure();
    }
}
