using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

/// <summary>
/// Unit tests for the <see cref="DefaultEchoExtractStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultEchoExtractStageTests : VKUnitTestBase
{

    [Fact]
    public async Task ExecuteAsync_WhenHistoryExists_InjectsEchoFragments()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var history = new List<VKEchoTrace>
        {
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("Message 1").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.Assistant).WithContent("Message 2").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("Message 3").Build()
        };

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());
        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ReturnsSuccessWithoutInjectingFragments()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var echoOptions = new VKEchoOptions { Enabled = false };
        var weavingOptions = new VKWeavingOptions();

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]));

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());
        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenEchoTierDisabledInArgs_ReturnsSuccessWithoutFetchingHistory()
    {
        // Arrange
        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithSessionId(sessionId)
            .WithRequestArgs(new VKWeavingArgs { DisabledTiers = [VKPromptTierType.Echo] })
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKEchoStore>().Verify(s => s.GetHistoryAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSessionIdIsEmpty_ReturnsSuccessEarly()
    {
        // Arrange
        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithSessionId(VKSessionId.Empty)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKEchoStore>().Verify(s => s.GetHistoryAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContinuousMode_TracesParentAncestry()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var parentSession = new VKSessionThreadBuilder()
            .WithMode(VKSessionMode.Continuous)
            .Build();

        var childSession = new VKSessionThreadBuilder()
            .WithMode(VKSessionMode.Continuous)
            .WithParentSessionId(parentSession.Id)
            .Build();

        var parentTrace = new VKEchoTraceBuilder()
            .WithSessionId(parentSession.Id)
            .WithRole(VKChatRole.User)
            .WithContent("Parent Msg")
            .Build();

        var childTrace = new VKEchoTraceBuilder()
            .WithSessionId(childSession.Id)
            .WithRole(VKChatRole.User)
            .WithContent("Child Msg")
            .Build();

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(childSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([childTrace]));

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(parentSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([parentTrace]));

        GetMock<IVKPsycheSessionRepository>()
            .Setup(s => s.FindByIdAsync(parentSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(parentSession));

        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithSessionId(childSession.Id)
            .BuildContext();
        context.SetState(childSession);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        fragments.Should().HaveCount(2);
        fragments[0].Segment.Content.Should().Be("Parent Msg");
        fragments[1].Segment.Content.Should().Be("Child Msg");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncludeSystemMessagesIsFalse_FiltersOutSystemRoleMessages()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var history = new List<VKEchoTrace>
        {
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.System).WithContent("System Injected Echo").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("User Question").Build()
        };

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var echoOptions = new VKEchoOptions { Enabled = true, IncludeSystemMessages = false };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        fragments.Should().ContainSingle();
        fragments[0].Segment.Role.Should().Be(VKChatRole.User);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMaxWindowSizeConfigured_LimitsRetainedEchoCount()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var history = Enumerable.Range(1, 10).Select(i => new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.User)
            .WithContent($"Msg {i}")
            .Build()).ToList();

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var echoOptions = new VKEchoOptions { Enabled = true, MaxWindowSize = 3 };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        fragments.Should().HaveCount(3);
        fragments[0].Segment.Content.Should().Be("Msg 8");
        fragments[1].Segment.Content.Should().Be("Msg 9");
        fragments[2].Segment.Content.Should().Be("Msg 10");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPruneUnitIsTurn_PrunesByTurnAndRespectsMaxTurns()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var history = new List<VKEchoTrace>
        {
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("T1 User").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.Assistant).WithContent("T1 Assistant").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("T2 User").Build(),
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.Assistant).WithContent("T2 Assistant").Build()
        };

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var echoOptions = new VKEchoOptions
        {
            Enabled = true,
            PruneUnit = VKEchoPruneUnit.Turn,
            MaxTurns = 1
        };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        fragments.Should().HaveCount(2);
        fragments[0].Segment.Content.Should().Be("T2 User");
        fragments[1].Segment.Content.Should().Be("T2 Assistant");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTierRenderOrderOverridesProvided_AppliesCustomRenderOrder()
    {
        // Arrange
        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 2048, ContextWindowSize = 4096 });

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var history = new List<VKEchoTrace>
        {
            new VKEchoTraceBuilder().WithSessionId(sessionId).WithRole(VKChatRole.User).WithContent("Msg").Build()
        };

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithSessionId(sessionId)
            .WithRequestArgs(new VKWeavingArgs
            {
                TierRenderOrderOverrides = [VKPromptTierType.Directive, VKPromptTierType.Echo]
            })
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragment = context.Fragments.First(f => f.TierType == VKPromptTierType.Echo);
        fragment.RenderOrder.Should().Be(10000);
    }
}
