using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class DefaultPersonaRendererTests : VKUnitTestBase
{
    [Fact]
    public void Render_RendersDescriptionAndTraits_WithoutName()
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
        result.Should().NotContain("Aegis");
        result.Should().Contain("Guardian AI");
        result.Should().Contain("- Tone: Professional");
        result.Should().Contain("- Role: Advisor");
    }

    [Fact]
    public void Render_TraitsAreSortedAlphabetically()
    {
        // Arrange
        var renderer = new DefaultPersonaRenderer();
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Aegis")
            .WithDescription("Guardian AI")
            .WithTraits(new Dictionary<string, string>
            {
                ["Zebra"] = "Last",
                ["Alpha"] = "First",
                ["Middle"] = "Between"
            })
            .Build();

        // Act
        var result = renderer.Render(persona);

        // Assert
        var alphaIndex = result.IndexOf("- Alpha: First", StringComparison.Ordinal);
        var middleIndex = result.IndexOf("- Middle: Between", StringComparison.Ordinal);
        var zebraIndex = result.IndexOf("- Zebra: Last", StringComparison.Ordinal);

        alphaIndex.Should().BeGreaterThan(-1);
        middleIndex.Should().BeGreaterThan(-1);
        zebraIndex.Should().BeGreaterThan(-1);
        alphaIndex.Should().BeLessThan(middleIndex);
        middleIndex.Should().BeLessThan(zebraIndex);
    }
}
