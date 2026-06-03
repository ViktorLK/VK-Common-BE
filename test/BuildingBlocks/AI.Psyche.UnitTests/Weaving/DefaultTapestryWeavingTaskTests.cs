using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultTapestryWeavingTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultTapestryWeavingTaskTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_ReturnsSuccess()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());
        var loggerMock = new Mock<ILogger<DefaultTapestryWeavingTask>>();

        var task = new DefaultTapestryWeavingTask(optionsMock.Object, loggerMock.Object);
        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
