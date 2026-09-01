using VK.Blocks.AI.Psyche.Common.Internal;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

public sealed class PromptPositionResolverTests : VKUnitTestBase
{
    [Fact]
    public void Resolve_WithAbsoluteDepth_ReturnsAbsoluteCoordinates()
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            Role = VKChatRole.System,
            AbsoluteDepth = 1,
            DepthPriority = 10
        };
        var orders = new Dictionary<VKPromptTierType, int>();

        // Act
        var coord = PromptPositionResolver.Resolve(segment, orders);

        // Assert
        coord.Role.Should().Be(VKChatRole.System);
        coord.RenderOrder.Should().Be(10);
    }

    [Fact]
    public void Resolve_WithRelativeDepth_CalculatesRelativeOrder()
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            Role = VKChatRole.User,
            RelativeDepth = VKPromptRelativeDepth.BeforeDirective,
            DepthPriority = 5
        };
        var orders = new Dictionary<VKPromptTierType, int>
        {
            [VKPromptTierType.Directive] = 10000
        };

        // Act
        var coord = PromptPositionResolver.Resolve(segment, orders);

        // Assert
        coord.Role.Should().Be(VKChatRole.System);
        coord.RenderOrder.Should().Be(10000 - PsycheConstants.Layout.RelativeOffset + 5);
    }

    [Theory]
    [InlineData(VKPromptRelativeDepth.AfterDirective, 10000 + PsycheConstants.Layout.RelativeOffset + 2)]
    [InlineData(VKPromptRelativeDepth.BeforePersona, 20000 - PsycheConstants.Layout.RelativeOffset + 2)]
    [InlineData(VKPromptRelativeDepth.AfterPersona, 20000 + PsycheConstants.Layout.RelativeOffset + 2)]
    [InlineData(VKPromptRelativeDepth.BeforeEcho, 30000 - PsycheConstants.Layout.RelativeOffset + 2)]
    [InlineData(VKPromptRelativeDepth.AfterEcho, 30000 + PsycheConstants.Layout.EchoReserve + 2)]
    public void Resolve_WithVariousRelativeDepths_CalculatesExpectedOrder(VKPromptRelativeDepth depth, int expectedOrder)
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            RelativeDepth = depth,
            DepthPriority = 2
        };
        var orders = new Dictionary<VKPromptTierType, int>
        {
            [VKPromptTierType.Directive] = 10000,
            [VKPromptTierType.Persona] = 20000,
            [VKPromptTierType.Echo] = 30000
        };

        // Act
        var coord = PromptPositionResolver.Resolve(segment, orders);

        // Assert
        coord.RenderOrder.Should().Be(expectedOrder);
    }

    [Fact]
    public void Resolve_WithInvalidRelativeDepth_ThrowsException()
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            RelativeDepth = (VKPromptRelativeDepth)99
        };
        var orders = new Dictionary<VKPromptTierType, int>();

        // Act
        System.Action act = () => PromptPositionResolver.Resolve(segment, orders);

        // Assert
        act.Should().Throw<System.ArgumentException>();
    }
}
