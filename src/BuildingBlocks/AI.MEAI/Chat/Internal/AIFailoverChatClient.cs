#pragma warning disable MEAI001

using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Mscc.GenerativeAI.Types;
using OllamaSharp.Models.Exceptions;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.AI.MEAI.Governance.Internal;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// A <see cref="FailoverChatClient"/> implementation that resolves candidate AI connections via <see cref="IVKAIRoutingPolicy"/>,
/// manages <see cref="IVKAICapacityLease"/> lifetimes, enforces cross-layer attempt caps, and reports invocation outcomes
/// to <see cref="IVKAIInvocationObserver"/>.
/// </summary>
// [ARCH-02] [AP.01] [CS.03] [CS.06] [MEA-07]
internal sealed class AIFailoverChatClient : FailoverChatClient
{
    private const string AffinityKeyProperty = "AffinityKey";
    private const string TurnIdProperty = "vk.turnId";
    private const string ScopeKeyProperty = "ScopeKey";
    private const string CorrelationTagsProperty = "CorrelationTags";
    private const string DefaultProfileName = "default";

    private readonly IChatClient _fallbackClient;
    private readonly IVKAIRoutingPolicy _routingPolicy;
    private readonly IVKAIInvocationObserver _invocationObserver;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly Func<VKAIRouteCandidate, IChatClient?>? _candidateClientResolver;
    private readonly int _maxTotalAttempts;
    private readonly Action? _onDispose;
    private readonly ConditionalWeakTable<RoutingContext, RequestRoutingState> _requestStates = new();

    public AIFailoverChatClient(
        IChatClient fallbackClient,
        IVKAIRoutingPolicy routingPolicy,
        IVKAIInvocationObserver invocationObserver,
        IVKGuidGenerator guidGenerator,
        TimeProvider? timeProvider = null,
        Func<VKAIRouteCandidate, IChatClient?>? candidateClientResolver = null,
        int maxTotalAttempts = 4,
        Action? onDispose = null)
    {
        _fallbackClient = VKGuard.NotNull(fallbackClient);
        _routingPolicy = VKGuard.NotNull(routingPolicy);
        _invocationObserver = VKGuard.NotNull(invocationObserver);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _candidateClientResolver = candidateClientResolver;
        _maxTotalAttempts = Math.Max(1, maxTotalAttempts);
        _onDispose = onDispose;
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
        => base.GetService(serviceType, serviceKey) ?? _fallbackClient.GetService(serviceType, serviceKey);

    protected override async ValueTask<IChatClient> SelectClientAsync(
        RoutingContext context,
        CancellationToken cancellationToken)
    {
        // [AP.01]
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_requestStates.TryGetValue(context, out RequestRoutingState? state))
        {
            return await SelectInitialClientAsync(context, cancellationToken).ConfigureAwait(false);
        }

        VKAIRouteSelection? next = state.PendingNextSelection;
        state.PendingNextSelection = null;

        if (next is null)
        {
            CleanupState(context, state);
            throw new InvalidOperationException("No pending failover candidate selection was available for the next attempt.");
        }

        try
        {
            return BindSelection(state, next);
        }
        catch
        {
            next.Dispose();
            CleanupState(context, state);
            throw;
        }
    }

    protected override async ValueTask OnRoutingUpdateAsync(
        RoutingContext context,
        FailoverChatClientAttempt attempt,
        bool isTerminal,
        CancellationToken cancellationToken)
    {
        // [AP.01]
        VKGuard.NotNull(context);
        VKGuard.NotNull(attempt);

        if (!_requestStates.TryGetValue(context, out RequestRoutingState? state))
        {
            return;
        }

        VKAIRouteSelection? selection = state.CurrentSelection;
        state.CurrentSelection = null;

        string connectionId = selection?.Candidate.ConnectionId
            ?? context.ChatOptions?.ModelId
            ?? DefaultProfileName;
        string? reservationId = selection?.ReservationId;
        int attemptNo = selection?.AttemptNo ?? Math.Max(1, state.CurrentAttemptNo);

        // Always release the capacity lease as soon as the attempt finishes.
        selection?.Dispose();

        AttemptScopedChatClient.AttemptTelemetry? telemetry = (attempt.Client as AttemptScopedChatClient)?.Telemetry;
        string invocationId = _guidGenerator.Create().ToString("N");

        if (attempt.Exception is null)
        {
            UsageDetails? usage = telemetry?.Usage;
            (int? cachedTokens, int? reasoningTokens) = ExtractTokenDetails(usage);

            VKAIInvocationOutcome outcome = new()
            {
                InvocationId = invocationId,
                AttemptNo = attemptNo,
                ConnectionId = connectionId,
                IsSuccess = attempt.ResponseCompleted,
                FailureKind = attempt.ResponseCompleted ? VKAIFailureKind.None : VKAIFailureKind.Cancelled,
                FailureDisposition = VKAIFailureDisposition.None,
                InputTokens = (int?)usage?.InputTokenCount,
                OutputTokens = (int?)usage?.OutputTokenCount,
                CachedTokens = cachedTokens,
                ReasoningTokens = reasoningTokens,
                CorrelationTags = state.OriginalRequest.CorrelationTags,
                HasToolSideEffects = telemetry?.HasToolSideEffects ?? false,
                Duration = attempt.Duration,
                TimeToFirstToken = attempt.TimeToFirstUpdate,
                ScopeKey = state.ScopeKey,
                FirstChunkCommitted = attempt.OutputCommitted,
                ReservationId = reservationId,
                AffinityKey = state.OriginalRequest.AffinityKey
            };

            CancellationToken observerToken = attempt.ResponseCompleted ? cancellationToken : CancellationToken.None;
            await ReportOutcomeSafeAsync(outcome, observerToken).ConfigureAwait(false);
            CleanupState(context, state);
            return;
        }

        Exception exception = attempt.Exception;
        (VKAIFailureKind failureKind, int? httpStatusCode, TimeSpan? retryAfter, VKAIFailureDisposition disposition) =
            ClassifyFailure(exception, telemetry, attempt.OutputCommitted, cancellationToken, _timeProvider);

        VKAIInvocationOutcome failureOutcome = new()
        {
            InvocationId = invocationId,
            AttemptNo = attemptNo,
            ConnectionId = connectionId,
            IsSuccess = false,
            FailureKind = failureKind,
            HttpStatusCode = httpStatusCode,
            RetryAfter = retryAfter,
            FailureDisposition = disposition,
            CorrelationTags = state.OriginalRequest.CorrelationTags,
            HasToolSideEffects = telemetry?.HasToolSideEffects ?? false,
            Duration = attempt.Duration,
            TimeToFirstToken = attempt.TimeToFirstUpdate,
            ScopeKey = state.ScopeKey,
            FirstChunkCommitted = attempt.OutputCommitted,
            ReservationId = reservationId,
            AffinityKey = state.OriginalRequest.AffinityKey
        };

        CancellationToken failureObserverToken = cancellationToken.IsCancellationRequested
            ? CancellationToken.None
            : cancellationToken;
        await ReportOutcomeSafeAsync(failureOutcome, failureObserverToken).ConfigureAwait(false);

        if (isTerminal)
        {
            CleanupState(context, state);
            return;
        }

        if (state.IsDirectFallback ||
            state.SharedResilienceContext.HasExceededMaxTotalAttempts ||
            selection is null)
        {
            CleanupState(context, state);
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        VKAIFailoverRequest failoverRequest = new()
        {
            OriginalRequest = state.OriginalRequest,
            Decision = selection.Decision,
            ExcludedConnectionIds = state.ExcludedConnectionIds,
            LastFailure = failureOutcome,
            NextAttemptNo = attemptNo + 1
        };

        VKResult<VKAIRouteSelection> nextResult;
        try
        {
            nextResult = await _routingPolicy.SelectNextCandidateAsync(failoverRequest, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            CleanupState(context, state);
            throw;
        }

        if (nextResult.IsFailure)
        {
            CleanupState(context, state);
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        state.PendingNextSelection = nextResult.Value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fallbackClient.Dispose();
            _onDispose?.Invoke();
        }

        base.Dispose(disposing);
    }

    private async ValueTask<IChatClient> SelectInitialClientAsync(
        RoutingContext context,
        CancellationToken cancellationToken)
    {
        (VKAIRouteRequest routeRequest, VKAIScopeKey? scopeKey) = BuildRouteRequest(context.ChatOptions);
        VKResilienceContext sharedContext = VKResilienceContext.Create(
            operationKey: $"ai:chat:{routeRequest.Profile}",
            traceId: routeRequest.CorrelationTags?.TraceId ?? Activity.Current?.TraceId.ToString(),
            tenantId: scopeKey?.Value,
            operationName: "AIMEAI.Chat.Failover").WithMaxTotalAttempts(_maxTotalAttempts);

        RequestRoutingState state = new()
        {
            OriginalRequest = routeRequest,
            ScopeKey = scopeKey,
            SharedResilienceContext = sharedContext
        };
        _requestStates.AddOrUpdate(context, state);

        if (_routingPolicy.IsNoOp)
        {
            state.IsDirectFallback = true;
            state.CurrentAttemptNo = 1;
            string fallbackConnectionId = context.ChatOptions?.ModelId ?? DefaultProfileName;
            return CreateAttemptClient(_fallbackClient, state, fallbackConnectionId, candidateModelId: null);
        }

        VKResult<VKAIRouteSelection> selectionResult;
        try
        {
            selectionResult = await _routingPolicy.SelectCandidateAsync(routeRequest, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            CleanupState(context, state);
            throw;
        }

        if (selectionResult.IsFailure)
        {
            state.IsDirectFallback = true;
            state.CurrentAttemptNo = 1;
            string fallbackConnectionId = context.ChatOptions?.ModelId ?? DefaultProfileName;
            return CreateAttemptClient(_fallbackClient, state, fallbackConnectionId, candidateModelId: null);
        }

        VKAIRouteSelection selection = selectionResult.Value;
        try
        {
            return BindSelection(state, selection);
        }
        catch
        {
            selection.Dispose();
            CleanupState(context, state);
            throw;
        }
    }

    private AttemptScopedChatClient BindSelection(RequestRoutingState state, VKAIRouteSelection selection)
    {
        state.CurrentSelection = selection;
        state.IsDirectFallback = false;
        state.CurrentAttemptNo = selection.AttemptNo;
        state.ExcludedConnectionIds.Add(selection.Candidate.ConnectionId);

        IChatClient targetClient = _candidateClientResolver?.Invoke(selection.Candidate) ?? _fallbackClient;
        string? candidateModelId = ResolveCandidateModelId(selection.Candidate);
        return CreateAttemptClient(targetClient, state, selection.Candidate.ConnectionId, candidateModelId);
    }

    private AttemptScopedChatClient CreateAttemptClient(
        IChatClient targetClient,
        RequestRoutingState state,
        string connectionId,
        string? candidateModelId)
    {
        VKResilienceContext attemptContext = state.SharedResilienceContext.WithConnectionId(connectionId);
        attemptContext.RetryAfter = null;
        attemptContext.FirstChunkCommitted = false;
        return new AttemptScopedChatClient(targetClient, attemptContext, candidateModelId, _timeProvider);
    }

    private void CleanupState(RoutingContext context, RequestRoutingState state)
    {
        state.CurrentSelection?.Dispose();
        state.CurrentSelection = null;
        state.PendingNextSelection?.Dispose();
        state.PendingNextSelection = null;
        _requestStates.Remove(context);
    }

    private async ValueTask ReportOutcomeSafeAsync(VKAIInvocationOutcome outcome, CancellationToken cancellationToken)
    {
        try
        {
            await _invocationObserver.ReportAsync(outcome, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Observer failures must not alter invocation outcomes or mask provider exceptions.
        }
    }

    private static string? ResolveCandidateModelId(VKAIRouteCandidate candidate)
        => !string.IsNullOrWhiteSpace(candidate.DeploymentName)
            ? candidate.DeploymentName
            : candidate.ModelId;

    private static (VKAIRouteRequest Request, VKAIScopeKey? ScopeKey) BuildRouteRequest(ChatOptions? options)
    {
        string? explicitAffinity = null;
        string? turnId = null;
        VKAIScopeKey? scopeKey = null;
        VKAICorrelationTags? correlationTags = null;

        if (options?.AdditionalProperties is not null)
        {
            if (options.AdditionalProperties.TryGetValue(AffinityKeyProperty, out object? affinityObj) &&
                affinityObj is string affinity &&
                !string.IsNullOrWhiteSpace(affinity))
            {
                explicitAffinity = affinity;
            }

            if (options.AdditionalProperties.TryGetValue(TurnIdProperty, out object? turnIdObj) &&
                turnIdObj is string turnIdStr &&
                !string.IsNullOrWhiteSpace(turnIdStr))
            {
                turnId = turnIdStr;
            }

            if (options.AdditionalProperties.TryGetValue(ScopeKeyProperty, out object? scopeObj) &&
                scopeObj is VKAIScopeKey scope)
            {
                scopeKey = scope;
            }

            if (options.AdditionalProperties.TryGetValue(CorrelationTagsProperty, out object? tagsObj) &&
                tagsObj is VKAICorrelationTags tags)
            {
                correlationTags = tags;
            }
        }

        string? affinityKey = explicitAffinity
            ?? turnId
            ?? (!string.IsNullOrWhiteSpace(correlationTags?.RunId) ? correlationTags.RunId : null)
            ?? (!string.IsNullOrWhiteSpace(correlationTags?.SessionId) ? correlationTags.SessionId : null);

        VKAIRouteRequest request = new()
        {
            Profile = !string.IsNullOrWhiteSpace(options?.ModelId) ? options.ModelId : DefaultProfileName,
            RequiresTools = options?.Tools is { Count: > 0 },
            RequiresStructuredOutput = options?.ResponseFormat is ChatResponseFormatJson,
            AffinityKey = affinityKey,
            ScopeKey = scopeKey,
            CorrelationTags = correlationTags
        };

        return (request, scopeKey);
    }

    private static (int? CachedTokens, int? ReasoningTokens) ExtractTokenDetails(UsageDetails? usage)
    {
        if (usage is null)
        {
            return (null, null);
        }

        int? cachedTokens = (int?)usage.CachedInputTokenCount
            ?? TryGetAdditionalCount(usage.AdditionalCounts, "CachedInputTokenCount", "cached_tokens");

        int? reasoningTokens = (int?)usage.ReasoningTokenCount
            ?? TryGetAdditionalCount(usage.AdditionalCounts, "ReasoningTokenCount", "reasoning_tokens", "OutputReasoningTokenCount");

        return (cachedTokens, reasoningTokens);
    }

    private static int? TryGetAdditionalCount(
        AdditionalPropertiesDictionary<long>? counts,
        params ReadOnlySpan<string> keys)
    {
        if (counts is null)
        {
            return null;
        }

        foreach (string key in keys)
        {
            if (counts.TryGetValue(key, out long value))
            {
                return (int)value;
            }
        }

        return null;
    }

    private static (VKAIFailureKind FailureKind, int? HttpStatusCode, TimeSpan? RetryAfter, VKAIFailureDisposition Disposition) ClassifyFailure(
        Exception ex,
        AttemptScopedChatClient.AttemptTelemetry? telemetry,
        bool outputCommitted,
        CancellationToken callerToken,
        TimeProvider timeProvider)
    {
        int? httpStatusCode = telemetry?.HttpStatusCode ?? ExtractHttpStatusCode(ex);
        TimeSpan? retryAfter = telemetry?.RetryAfter ?? AIMEAIRetryAfterHelper.Extract(ex, timeProvider);
        VKAIFailureDisposition disposition = telemetry?.FailureDisposition ?? VKAIFailureDisposition.None;

        if (ex is OperationCanceledException && callerToken.IsCancellationRequested)
        {
            return (VKAIFailureKind.Cancelled, httpStatusCode, retryAfter, VKAIFailureDisposition.None);
        }

        if (outputCommitted)
        {
            return (VKAIFailureKind.StreamInterrupted, httpStatusCode, retryAfter, VKAIFailureDisposition.OutcomeUnknown);
        }

        if (ex is TimeoutException or GeminiApiTimeoutException || (ex is OperationCanceledException && !callerToken.IsCancellationRequested))
        {
            return (VKAIFailureKind.Timeout, httpStatusCode, retryAfter, VKAIFailureDisposition.OutcomeUnknown);
        }

        if (ex is AISecurityException)
        {
            return (VKAIFailureKind.ContentRejected, httpStatusCode, retryAfter, VKAIFailureDisposition.TerminalRejected);
        }

        if (httpStatusCode.HasValue)
        {
            VKAIFailureKind byStatus = httpStatusCode.Value switch
            {
                (int)HttpStatusCode.TooManyRequests => VKAIFailureKind.RateLimited,
                (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden => VKAIFailureKind.AuthFailed,
                (int)HttpStatusCode.RequestTimeout => VKAIFailureKind.Timeout,
                >= 400 and < 500 => VKAIFailureKind.BadRequest,
                >= (int)HttpStatusCode.InternalServerError => VKAIFailureKind.ServerError,
                _ => VKAIFailureKind.Transport
            };

            if (disposition == VKAIFailureDisposition.None)
            {
                disposition = httpStatusCode.Value switch
                {
                    (int)HttpStatusCode.TooManyRequests
                        or (int)HttpStatusCode.Unauthorized
                        or (int)HttpStatusCode.Forbidden
                        or (int)HttpStatusCode.ServiceUnavailable
                        or (int)HttpStatusCode.BadGateway => VKAIFailureDisposition.RequestNotAccepted,
                    (int)HttpStatusCode.RequestTimeout
                        or (int)HttpStatusCode.GatewayTimeout => VKAIFailureDisposition.OutcomeUnknown,
                    >= 400 and < 500 => VKAIFailureDisposition.TerminalRejected,
                    _ => VKAIFailureDisposition.None
                };
            }

            return (byStatus, httpStatusCode, retryAfter, disposition);
        }

        if (ex is OllamaException)
        {
            return (VKAIFailureKind.BadRequest, (int)HttpStatusCode.BadRequest, retryAfter, VKAIFailureDisposition.TerminalRejected);
        }

        if (ex is HttpRequestException httpEx)
        {
            if (disposition == VKAIFailureDisposition.None)
            {
                disposition = httpEx.HttpRequestError switch
                {
                    HttpRequestError.NameResolutionError
                        or HttpRequestError.ConnectionError
                        or HttpRequestError.SecureConnectionError
                        or HttpRequestError.ProxyTunnelError => VKAIFailureDisposition.RequestNotAccepted,
                    _ => VKAIFailureDisposition.OutcomeUnknown
                };
            }

            return (VKAIFailureKind.Transport, null, retryAfter, disposition);
        }

        return (VKAIFailureKind.ServerError, null, retryAfter, disposition);
    }

    private static int? ExtractHttpStatusCode(Exception ex) => ex switch
    {
        ClientResultException clientEx when clientEx.Status > 0 => clientEx.Status,
        GeminiApiTimeoutException geminiTimeoutEx when geminiTimeoutEx.Response is not null => (int)geminiTimeoutEx.Response.StatusCode,
        GeminiApiException geminiEx when geminiEx.Response is not null => (int)geminiEx.Response.StatusCode,
        HttpRequestException httpEx when httpEx.StatusCode.HasValue => (int)httpEx.StatusCode.Value,
        _ => null
    };

    private sealed class RequestRoutingState
    {
        public required VKAIRouteRequest OriginalRequest { get; init; }
        public required VKAIScopeKey? ScopeKey { get; init; }
        public required VKResilienceContext SharedResilienceContext { get; init; }
        public HashSet<string> ExcludedConnectionIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public VKAIRouteSelection? CurrentSelection { get; set; }
        public VKAIRouteSelection? PendingNextSelection { get; set; }
        public bool IsDirectFallback { get; set; }
        public int CurrentAttemptNo { get; set; }
    }
}
