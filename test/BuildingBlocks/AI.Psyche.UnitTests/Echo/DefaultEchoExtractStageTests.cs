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
    public DefaultEchoExtractStageTests()
    {
        GetMock<IVKEchoRenderer>()
            .Setup(r => r.Render(It.IsAny<VKEchoTrace>(), It.IsAny<VKPsycheContext>()))
            .Returns((VKEchoTrace t, VKPsycheContext _) => t.Content);
    }

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
        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var session = new VKSessionThreadBuilder().Build();
        var sessionId = session.Id;
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
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());
        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Echoes.Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ReturnsSuccessWithoutInjectingFragments()
    {
        // Arrange
        var echoOptions = new VKEchoOptions { Enabled = false };
        var weavingOptions = new VKWeavingOptions();

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        SetupEchoStore(sessionId, []);

        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());
        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Echoes.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabledInArgs_ReturnsSuccessWithoutFetchingHistory()
    {
        // Arrange
        var echoOptions = new VKEchoOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var stage = new DefaultEchoExtractStage(
            GetMockObject<IVKEchoStore>(),
            GetMockObject<IVKPsycheSessionRepository>(),
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithSessionId(sessionId)
            .WithRequestArgs(new VKEchoArgs { Enabled = false })
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
            GetMockObject<IVKEchoRenderer>(),
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
            GetMockObject<IVKEchoRenderer>(),
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
        context.Echoes.Should().HaveCount(2);
        context.Echoes[0].Content.Should().Be("Parent Msg");
        context.Echoes[1].Content.Should().Be("Child Msg");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncludeSystemMessagesIsFalse_FiltersOutSystemRoleMessages()
    {
        // Arrange
        var session = new VKSessionThreadBuilder().Build();
        var sessionId = session.Id;
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
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Echoes.Should().ContainSingle();
        context.Echoes[0].Role.Should().Be(VKChatRole.User);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMaxWindowSizeConfigured_LimitsRetainedEchoCount()
    {
        // Arrange
        var session = new VKSessionThreadBuilder().Build();
        var sessionId = session.Id;
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
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Echoes.Should().HaveCount(3);
        context.Echoes[0].Content.Should().Be("Msg 8");
        context.Echoes[1].Content.Should().Be("Msg 9");
        context.Echoes[2].Content.Should().Be("Msg 10");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPruneUnitIsTurn_PrunesByTurnAndRespectsMaxTurns()
    {
        // Arrange
        var session = new VKSessionThreadBuilder().Build();
        var sessionId = session.Id;
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
            GetMockObject<IVKEchoRenderer>(),
            echoOptions,
            weavingOptions,
            GetMockObject<ILogger<DefaultEchoExtractStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithSessionId(sessionId).BuildContext();
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Echoes.Should().HaveCount(2);
        context.Echoes[0].Content.Should().Be("T2 User");
        context.Echoes[1].Content.Should().Be("T2 Assistant");
    }
}
