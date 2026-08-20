using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

/// <summary>
/// Unit tests for the <see cref="DefaultEchoExtractStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultEchoStageTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(
        VKSessionId sessionId,
        string userInput = "how are you?")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            SessionId = sessionId,
            UserInput = userInput
        };

        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHistoryExists_InjectsEchoFragments()
    {
        // Arrange
        var echoStoreMock = new Mock<IVKEchoStore>();
        var sessionStoreMock = new Mock<IVKSessionStore>();
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        var modelCatalogMock = new Mock<IVKModelCatalog>();
        modelCatalogMock.Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();
        var loggerMock = new Mock<ILogger<DefaultEchoExtractStage>>();

        var sessionId = new VKSessionId(Guid.NewGuid());
        var history = new List<VKEchoTrace>
        {
            new() { SessionId = sessionId, Id = new VKEchoId(Guid.NewGuid()), Role = VKChatRole.User, Content = "Message 1" },
            new() { SessionId = sessionId, Id = new VKEchoId(Guid.NewGuid()), Role = VKChatRole.Assistant, Content = "Message 2" },
            new() { SessionId = sessionId, Id = new VKEchoId(Guid.NewGuid()), Role = VKChatRole.User, Content = "Message 3" }
        };

        echoStoreMock.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var stage = new DefaultEchoExtractStage(
            echoStoreMock.Object,
            sessionStoreMock.Object,
            tokenCounterMock.Object,
            modelCatalogMock.Object,
            echoOptions,
            weavingOptions,
            loggerMock.Object);
        var (context, _) = CreateTestContext(sessionId);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ReturnsSuccessWithoutInjectingFragments()
    {
        // Arrange
        var echoStoreMock = new Mock<IVKEchoStore>();
        var sessionStoreMock = new Mock<IVKSessionStore>();
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        var modelCatalogMock = new Mock<IVKModelCatalog>();
        modelCatalogMock.Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var echoOptions = new VKEchoOptions { Enabled = false };
        var weavingOptions = new VKWeavingOptions();
        var loggerMock = new Mock<ILogger<DefaultEchoExtractStage>>();

        var sessionId = new VKSessionId(Guid.NewGuid());
        echoStoreMock.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]));

        var stage = new DefaultEchoExtractStage(
            echoStoreMock.Object,
            sessionStoreMock.Object,
            tokenCounterMock.Object,
            modelCatalogMock.Object,
            echoOptions,
            weavingOptions,
            loggerMock.Object);
        var (context, _) = CreateTestContext(sessionId);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).Should().BeEmpty();
    }
}
