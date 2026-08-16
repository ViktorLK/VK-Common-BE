using System;
using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class DefaultPersonaRendererTests
{
    [Fact]
    public void Render_RendersNameDescriptionAndTraits()
    {
        // Arrange
        var renderer = new DefaultPersonaRenderer();
        var persona = new VKPersonaAnchor
        {
            Id = new VKPersonaId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Name = "Aegis",
            Description = "Guardian AI",
            Traits = new Dictionary<string, string>
            {
                ["Tone"] = "Professional",
                ["Role"] = "Advisor"
            }
        };

        // Act
        var result = renderer.Render(persona);

        // Assert
        result.Should().Contain("Aegis");
        result.Should().Contain("Guardian AI");
        result.Should().Contain("- Tone: Professional");
        result.Should().Contain("- Role: Advisor");
    }
}
