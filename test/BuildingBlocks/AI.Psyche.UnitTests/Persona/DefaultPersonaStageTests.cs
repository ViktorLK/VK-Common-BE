using Microsoft.Extensions.DependencyInjection;
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
    public async Task ExecuteAsync_HappyPath_AddsPersonaSegment()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Tester")
            .WithDescription("Friendly bot")
            .Build();

        GetMock<IVKPsychePersonaRepository>()
            .Setup(s => s.FindByIdAsync(persona.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(persona));

        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(persona))
            .Returns("Rendered Persona Text");

        var personaOptions = new VKPersonaOptions { Enabled = true };
        var stage = new DefaultPersonaStage(
            personaOptions,
            GetMockObject<IVKPsychePersonaRepository>(),
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithPersonaId(persona.Id)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle(s => s.Tier == VKPromptTierType.Persona).Subject;
        segment.Content.Should().Be("Rendered Persona Text");
        context.State<VKPersonaAnchor>().Should().Be(persona);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotAddSegments()
    {
        // Arrange
        var stage = new DefaultPersonaStage(
            new VKPersonaOptions { Enabled = false },
            GetMockObject<IVKPsychePersonaRepository>(),
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithPersonaId(new VKPersonaId(Guid.NewGuid()))
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPersonaId_ReturnsSuccessImmediately()
    {
        // Arrange
        var stage = new DefaultPersonaStage(
            new VKPersonaOptions { Enabled = true },
            GetMockObject<IVKPsychePersonaRepository>(),
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var request = new VKPsycheRequest
        {
            PersonaId = null,
            UserInput = "hello"
        };
        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Services = new ServiceCollection().BuildServiceProvider()
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var personaId = new VKPersonaId(Guid.NewGuid());
        GetMock<IVKPsychePersonaRepository>()
            .Setup(s => s.FindByIdAsync(personaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound));

        var stage = new DefaultPersonaStage(
            new VKPersonaOptions { Enabled = true },
            GetMockObject<IVKPsychePersonaRepository>(),
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithPersonaId(personaId)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRendererReturnsWhitespace_SkipsSegment()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();

        GetMock<IVKPsychePersonaRepository>()
            .Setup(s => s.FindByIdAsync(persona.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(persona));

        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(persona))
            .Returns("   ");

        var stage = new DefaultPersonaStage(
            new VKPersonaOptions { Enabled = true },
            GetMockObject<IVKPsychePersonaRepository>(),
            GetMockObject<IVKPersonaRenderer>(),
            GetMockObject<ILogger<DefaultPersonaStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithPersonaId(persona.Id)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
        context.State<VKPersonaAnchor>().Should().Be(persona);
    }
}
