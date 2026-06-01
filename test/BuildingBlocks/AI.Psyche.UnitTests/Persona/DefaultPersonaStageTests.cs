using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

/// <summary>
/// Unit tests for the <see cref="DefaultPersonaStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPersonaStageTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsPersonaFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKPersonaStore>();
        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var persona = new VKPersonaAnchor
        {
            Id = "test-persona",
            Name = "Tester",
            Description = "Friendly bot"
        };
        storeMock.Setup(s => s.GetPersonaAsync("test-persona", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(persona));

        var loggerMock = new Mock<ILogger<DefaultPersonaStage>>();
        var stage = new DefaultPersonaStage(storeMock.Object, optionsMock.Object, loggerMock.Object);
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
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Persona).Subject;
        fragment.Metadata.Should().Be(persona);
    }
}
