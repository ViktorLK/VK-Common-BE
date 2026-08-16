using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

/// <summary>
/// Unit tests for the <see cref="DefaultPersonaStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPersonaStageTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(
        string personaId = "test-persona")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = "hello"
        };

        var context = new VKPsycheContext
        {
            Request = request,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsPersonaFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKPersonaStore>();
        var personaOptions = new VKPersonaOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var personaId = new VKPersonaId(Guid.NewGuid());
        var persona = new VKPersonaAnchor
        {
            TenantId = VKTenantId.Default,
            Id = personaId,
            Name = "Tester",
            Description = "Friendly bot"
        };
        storeMock.Setup(s => s.GetPersonaAsync(It.IsAny<VKPersonaId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(persona));

        var loggerMock = new Mock<ILogger<DefaultPersonaStage>>();
        var stage = new DefaultPersonaStage(personaOptions, storeMock.Object, weavingOptions, loggerMock.Object);
        var (context, _) = CreateTestContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Persona).Subject;
        fragment.Metadata.Should().Be(persona);
    }
}
