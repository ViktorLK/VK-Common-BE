using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private void SetupEchoStore(VKSessionId sessionId, List<VKEchoTrace> traces)
    {
        var metas = traces.Select(t => new VKEchoMetadata
        {
            Id = t.Id,
            SessionId = t.SessionId,
            Role = t.Role,
            TokenCount = t.TokenCount,
            CreatedAt = t.CreatedAt
        }).ToList();

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetMetadataAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>(metas));

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetTracesByIdsAsync(It.IsAny<IReadOnlyCollection<VKEchoId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<VKEchoId> ids, CancellationToken _) =>
            {
                var idSet = new HashSet<VKEchoId>(ids);
                return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(traces.Where(t => idSet.Contains(t.Id)).ToList());
            });
    }

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

        SetupEchoStore(sessionId, history);

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
        SetupEchoStore(sessionId, []);

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
        GetMock<IVKEchoStore>().Verify(s => s.GetMetadataAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()), Times.Never);
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
        GetMock<IVKEchoStore>().Verify(s => s.GetMetadataAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()), Times.Never);
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

        var allTraces = new List<VKEchoTrace> { parentTrace, childTrace };

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetMetadataAsync(childSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>([
                new VKEchoMetadata { Id = childTrace.Id, SessionId = childTrace.SessionId, Role = childTrace.Role, TokenCount = 10, CreatedAt = childTrace.CreatedAt }
            ]));

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetMetadataAsync(parentSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>([
                new VKEchoMetadata { Id = parentTrace.Id, SessionId = parentTrace.SessionId, Role = parentTrace.Role, TokenCount = 10, CreatedAt = parentTrace.CreatedAt }
            ]));

        GetMock<IVKEchoStore>()
            .Setup(s => s.GetTracesByIdsAsync(It.IsAny<IReadOnlyCollection<VKEchoId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<VKEchoId> ids, CancellationToken _) =>
            {
                var idSet = new HashSet<VKEchoId>(ids);
                return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(allTraces.Where(t => idSet.Contains(t.Id)).ToList());
            });

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

        SetupEchoStore(sessionId, history);

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

        SetupEchoStore(sessionId, history);

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

        SetupEchoStore(sessionId, history);

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

        SetupEchoStore(sessionId, history);

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
