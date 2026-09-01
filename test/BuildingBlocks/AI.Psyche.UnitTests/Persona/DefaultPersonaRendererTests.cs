using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class DefaultPersonaRendererTests : VKUnitTestBase
{
    [Fact]
    public void Render_RendersNameDescriptionAndTraits()
    {
        // Arrange
        var renderer = new DefaultPersonaRenderer();
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Aegis")
            .WithDescription("Guardian AI")
            .WithTraits(new Dictionary<string, string>
            {
                ["Tone"] = "Professional",
                ["Role"] = "Advisor"
            })
            .Build();

        // Act
        var result = renderer.Render(persona);

        // Assert
        result.Should().Contain("Aegis");
        result.Should().Contain("Guardian AI");
        result.Should().Contain("- Tone: Professional");
        result.Should().Contain("- Role: Advisor");
    }
}
