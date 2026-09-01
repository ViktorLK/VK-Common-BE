using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

/// <summary>
/// Unit tests for the <see cref="DefaultPersonaStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPersonaStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsPersonaFragment()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Tester")
            .WithDescription("Friendly bot")
            .Build();

        GetMock<IVKPsychePersonaRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPersonaId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKPersonaAnchor>>([persona]));

        var personaOptions = new VKPersonaOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();
        var stage = new DefaultPersonaStage(
            personaOptions,
            GetMockObject<IVKPsychePersonaRepository>(),
            weavingOptions,
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithPersonaId(persona.Id)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Persona).Subject;
        fragment.Metadata.Should().Be(persona);
    }
}
