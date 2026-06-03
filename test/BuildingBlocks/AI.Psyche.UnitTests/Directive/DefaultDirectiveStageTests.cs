using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

/// <summary>
/// Unit tests for the <see cref="DefaultDirectiveStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultDirectiveStageTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsDirectiveFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var directive = new VKDirectiveCharter
        {
            TenantId = "test-tenant",
            Overview = "Test Safety Rulebook"
        };
        storeMock.Setup(s => s.GetDirectiveAsync("test-tenant", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(directive));

        var stage = new DefaultDirectiveStage(storeMock.Object, loggerMock.Object, optionsMock.Object);
        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await stage.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Directive).Subject;
        fragment.Metadata.Should().Be(directive);
    }
}
