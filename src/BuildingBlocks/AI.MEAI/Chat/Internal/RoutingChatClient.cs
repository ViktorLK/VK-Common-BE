using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// A <see cref="DelegatingChatClient"/> that resolves candidate AI connections via <see cref="IVKAIRoutingPolicy"/>,
/// performs bounded failover across candidates, and reports invocation outcomes to <see cref="IVKAIInvocationObserver"/>.
/// </summary>
// [ARCH-02] [AP.01] [CS.03] [CS.06] [MEA-07]
internal sealed class RoutingChatClient : DelegatingChatClient
{
    private const string AffinityKeyProperty = "AffinityKey";
    private const string ScopeKeyProperty = "ScopeKey";
    private const string CorrelationTagsProperty = "CorrelationTags";
    private const string DefaultProfileName = "default";

    private readonly IVKAIRoutingPolicy _routingPolicy;
    private readonly IVKAIInvocationObserver _invocationObserver;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly Func<VKAIRouteCandidate, IChatClient?>? _candidateClientResolver;

    public RoutingChatClient(
        IChatClient innerClient,
        IVKAIRoutingPolicy routingPolicy,
        IVKAIInvocationObserver invocationObserver,
        IVKGuidGenerator guidGenerator,
        TimeProvider? timeProvider = null,
        Func<VKAIRouteCandidate, IChatClient?>? candidateClientResolver = null)
        : base(VKGuard.NotNull(innerClient))
    {
        _routingPolicy = VKGuard.NotNull(routingPolicy);
        _invocationObserver = VKGuard.NotNull(invocationObserver);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _candidateClientResolver = candidateClientResolver;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);
        var (routeRequest, scopeKey) = BuildRouteRequest(options);

        var decisionResult = await _routingPolicy.DecideAsync(routeRequest, cancellationToken).ConfigureAwait(false);
        if (decisionResult.IsFailure || decisionResult.Value.Candidates.Count == 0)
        {
            var fallbackConnectionId = options?.ModelId ?? DefaultProfileName;
            return await InvokeAndObserveAsync(
                InnerClient,
                messages,
                options,
                attemptNo: 1,
                fallbackConnectionId,
                routeRequest.CorrelationTags,
                scopeKey,
                cancellationToken).ConfigureAwait(false);
        }

        var decision = decisionResult.Value;
        var maxAttempts = Math.Min(decision.Candidates.Count, Math.Max(1, decision.MaxFailoverAttempts));

        for (var i = 0; i < maxAttempts; i++)
        {
            var candidate = decision.Candidates[i];
            var targetClient = ResolveCandidateClient(candidate);
            var candidateOptions = CreateCandidateOptions(options, candidate);

            try
            {
                return await InvokeAndObserveAsync(
                    targetClient,
                    messages,
                    candidateOptions,
                    attemptNo: i + 1,
                    candidate.ConnectionId,
                    routeRequest.CorrelationTags,
                    scopeKey,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var failureKind = ClassifyFailure(ex);
                if (!IsSwitchable(decision, failureKind) || i == maxAttempts - 1)
                {
                    throw;
                }
            }
        }

        return await InnerClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);
        var (routeRequest, scopeKey) = BuildRouteRequest(options);

        var decisionResult = await _routingPolicy.DecideAsync(routeRequest, cancellationToken).ConfigureAwait(false);
        var decision = decisionResult.IsSuccess && decisionResult.Value.Candidates.Count > 0
            ? decisionResult.Value
            : null;
        var maxAttempts = decision is not null
            ? Math.Min(decision.Candidates.Count, Math.Max(1, decision.MaxFailoverAttempts))
            : 1;

        for (var i = 0; i < maxAttempts; i++)
        {
            var candidate = decision is not null ? decision.Candidates[i] : null;
            var targetClient = candidate is not null ? ResolveCandidateClient(candidate) : InnerClient;
            var targetOptions = candidate is not null ? CreateCandidateOptions(options, candidate) : options;
            var connectionId = candidate?.ConnectionId ?? options?.ModelId ?? DefaultProfileName;

            var invocationId = _guidGenerator.Create().ToString("N");
            var startTimestamp = _timeProvider.GetTimestamp();

            var firstChunkYielded = false;
            TimeSpan? timeToFirstToken = null;
            UsageDetails? streamUsage = null;
            var hasToolSideEffects = false;
            Exception? caughtException = null;

            IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
            try
            {
                enumerator = targetClient.GetStreamingResponseAsync(messages, targetOptions, cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            if (enumerator is not null)
            {
                try
                {
                    while (true)
                    {
                        bool movedNext;
                        try
                        {
                            movedNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                            break;
                        }

                        if (!movedNext)
                        {
                            break;
                        }

                        var update = enumerator.Current;
                        if (!firstChunkYielded)
                        {
                            firstChunkYielded = true;
                            timeToFirstToken = _timeProvider.GetElapsedTime(startTimestamp);
                        }

                        if (update.Contents is not null)
                        {
                            foreach (var content in update.Contents)
                            {
                                if (content is UsageContent usageContent && usageContent.Details is not null)
                                {
                                    streamUsage = usageContent.Details;
                                }
                                else if (content is FunctionCallContent)
                                {
                                    hasToolSideEffects = true;
                                }
                            }
                        }

                        yield return update;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            var duration = _timeProvider.GetElapsedTime(startTimestamp);

            if (caughtException is OperationCanceledException oce && cancellationToken.IsCancellationRequested)
            {
                await ReportOutcomeSafeAsync(
                    new VKAIInvocationOutcome
                    {
                        InvocationId = invocationId,
                        AttemptNo = i + 1,
                        ConnectionId = connectionId,
                        IsSuccess = false,
                        FailureKind = VKAIFailureKind.Cancelled,
                        CorrelationTags = routeRequest.CorrelationTags,
                        Duration = duration,
                        TimeToFirstToken = timeToFirstToken,
                        ScopeKey = scopeKey
                    },
                    CancellationToken.None).ConfigureAwait(false);

                ExceptionDispatchInfo.Capture(oce).Throw();
            }

            if (caughtException is not null)
            {
                var failureKind = ClassifyFailure(caughtException);
                await ReportOutcomeSafeAsync(
                    new VKAIInvocationOutcome
                    {
                        InvocationId = invocationId,
                        AttemptNo = i + 1,
                        ConnectionId = connectionId,
                        IsSuccess = false,
                        FailureKind = failureKind,
                        CorrelationTags = routeRequest.CorrelationTags,
                        Duration = duration,
                        TimeToFirstToken = timeToFirstToken,
                        ScopeKey = scopeKey
                    },
                    cancellationToken).ConfigureAwait(false);

                if (firstChunkYielded || decision is null || !IsSwitchable(decision, failureKind) || i == maxAttempts - 1)
                {
                    ExceptionDispatchInfo.Capture(caughtException).Throw();
                }

                continue;
            }

            var (cachedTokens, reasoningTokens) = ExtractTokenDetails(streamUsage);

            await ReportOutcomeSafeAsync(
                new VKAIInvocationOutcome
                {
                    InvocationId = invocationId,
                    AttemptNo = i + 1,
                    ConnectionId = connectionId,
                    IsSuccess = true,
                    FailureKind = VKAIFailureKind.None,
                    InputTokens = (int?)streamUsage?.InputTokenCount,
                    OutputTokens = (int?)streamUsage?.OutputTokenCount,
                    CachedTokens = cachedTokens,
                    ReasoningTokens = reasoningTokens,
                    CorrelationTags = routeRequest.CorrelationTags,
                    HasToolSideEffects = hasToolSideEffects,
                    Duration = duration,
                    TimeToFirstToken = timeToFirstToken,
                    ScopeKey = scopeKey
                },
                cancellationToken).ConfigureAwait(false);

            yield break;
        }
    }

    // [MEA-07] Observe actual provider usage and failure telemetry across routed and direct invocations.
    private async Task<ChatResponse> InvokeAndObserveAsync(
        IChatClient targetClient,
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        int attemptNo,
        string connectionId,
        VKAICorrelationTags? correlationTags,
        VKAIScopeKey? scopeKey,
        CancellationToken cancellationToken)
    {
        var invocationId = _guidGenerator.Create().ToString("N");
        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            var response = await targetClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            var duration = _timeProvider.GetElapsedTime(startTimestamp);
            var usageDetails = ExtractUsageDetails(response);
            var (cachedTokens, reasoningTokens) = ExtractTokenDetails(usageDetails);
            var hasToolSideEffects = HasFunctionCall(response.Messages);

            await ReportOutcomeSafeAsync(
                new VKAIInvocationOutcome
                {
                    InvocationId = invocationId,
                    AttemptNo = attemptNo,
                    ConnectionId = connectionId,
                    IsSuccess = true,
                    FailureKind = VKAIFailureKind.None,
                    InputTokens = (int?)usageDetails?.InputTokenCount,
                    OutputTokens = (int?)usageDetails?.OutputTokenCount,
                    CachedTokens = cachedTokens,
                    ReasoningTokens = reasoningTokens,
                    CorrelationTags = correlationTags,
                    HasToolSideEffects = hasToolSideEffects,
                    Duration = duration,
                    ScopeKey = scopeKey
                },
                cancellationToken).ConfigureAwait(false);

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var duration = _timeProvider.GetElapsedTime(startTimestamp);
            await ReportOutcomeSafeAsync(
                new VKAIInvocationOutcome
                {
                    InvocationId = invocationId,
                    AttemptNo = attemptNo,
                    ConnectionId = connectionId,
                    IsSuccess = false,
                    FailureKind = VKAIFailureKind.Cancelled,
                    CorrelationTags = correlationTags,
                    Duration = duration,
                    ScopeKey = scopeKey
                },
                CancellationToken.None).ConfigureAwait(false);

            throw;
        }
        catch (Exception ex)
        {
            var duration = _timeProvider.GetElapsedTime(startTimestamp);
            var failureKind = ClassifyFailure(ex);
            await ReportOutcomeSafeAsync(
                new VKAIInvocationOutcome
                {
                    InvocationId = invocationId,
                    AttemptNo = attemptNo,
                    ConnectionId = connectionId,
                    IsSuccess = false,
                    FailureKind = failureKind,
                    CorrelationTags = correlationTags,
                    Duration = duration,
                    ScopeKey = scopeKey
                },
                cancellationToken).ConfigureAwait(false);

            throw;
        }
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

    private IChatClient ResolveCandidateClient(VKAIRouteCandidate candidate)
        => _candidateClientResolver?.Invoke(candidate) ?? InnerClient;

    private static (VKAIRouteRequest Request, VKAIScopeKey? ScopeKey) BuildRouteRequest(ChatOptions? options)
    {
        string? affinityKey = null;
        VKAIScopeKey? scopeKey = null;
        VKAICorrelationTags? correlationTags = null;

        if (options?.AdditionalProperties is not null)
        {
            if (options.AdditionalProperties.TryGetValue(AffinityKeyProperty, out var affinityObj) && affinityObj is string affinity)
            {
                affinityKey = affinity;
            }

            if (options.AdditionalProperties.TryGetValue(ScopeKeyProperty, out var scopeObj) && scopeObj is VKAIScopeKey scope)
            {
                scopeKey = scope;
            }

            if (options.AdditionalProperties.TryGetValue(CorrelationTagsProperty, out var tagsObj) && tagsObj is VKAICorrelationTags tags)
            {
                correlationTags = tags;
            }
        }

        var request = new VKAIRouteRequest
        {
            Profile = options?.ModelId ?? DefaultProfileName,
            RequiresTools = options?.Tools is { Count: > 0 },
            RequiresStructuredOutput = options?.ResponseFormat is ChatResponseFormatJson,
            AffinityKey = affinityKey,
            ScopeKey = scopeKey,
            CorrelationTags = correlationTags
        };

        return (request, scopeKey);
    }

    private static UsageDetails? ExtractUsageDetails(ChatResponse? response)
    {
        if (response is null)
        {
            return null;
        }

        if (response.Usage is not null)
        {
            return response.Usage;
        }

        if (response.Messages is null)
        {
            return null;
        }

        UsageDetails? found = null;
        foreach (var message in response.Messages)
        {
            if (message.Contents is null)
            {
                continue;
            }

            foreach (var content in message.Contents)
            {
                if (content is UsageContent uc && uc.Details is not null)
                {
                    found = uc.Details;
                }
            }
        }

        return found;
    }

    private static (int? CachedTokens, int? ReasoningTokens) ExtractTokenDetails(UsageDetails? usage)
    {
        if (usage is null)
        {
            return (null, null);
        }

        var cachedTokens = (int?)usage.CachedInputTokenCount
            ?? TryGetAdditionalCount(usage.AdditionalCounts, "CachedInputTokenCount", "cached_tokens");

        var reasoningTokens = (int?)usage.ReasoningTokenCount
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

        foreach (var key in keys)
        {
            if (counts.TryGetValue(key, out var value))
            {
                return (int)value;
            }
        }

        return null;
    }

    private static bool HasFunctionCall(IList<ChatMessage>? messages)
    {
        if (messages is null)
        {
            return false;
        }

        foreach (var message in messages)
        {
            if (message.Contents is null)
            {
                continue;
            }

            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsSwitchable(VKAIRouteDecision decision, VKAIFailureKind failureKind)
    {
        if (decision.SwitchableFailures is not null &&
            decision.SwitchableFailures.TryGetValue(failureKind, out var isSwitchable))
        {
            return isSwitchable;
        }

        return failureKind is not (VKAIFailureKind.BadRequest or VKAIFailureKind.ContentRejected or VKAIFailureKind.Cancelled);
    }

    private static ChatOptions CreateCandidateOptions(ChatOptions? source, VKAIRouteCandidate candidate)
    {
        var modelId = !string.IsNullOrWhiteSpace(candidate.DeploymentName)
            ? candidate.DeploymentName
            : candidate.ModelId;
        return CreateCandidateOptions(source, modelId);
    }

    private static ChatOptions CreateCandidateOptions(ChatOptions? source, string modelId)
    {
        if (source is null)
        {
            return new ChatOptions { ModelId = modelId };
        }

        var cloned = source.Clone();
        cloned.ModelId = modelId;
        return cloned;
    }

    private static VKAIFailureKind ClassifyFailure(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => VKAIFailureKind.Cancelled,
            TimeoutException => VKAIFailureKind.Transport,
            ClientResultException clientEx => clientEx.Status switch
            {
                (int)HttpStatusCode.TooManyRequests => VKAIFailureKind.RateLimited,
                (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden => VKAIFailureKind.AuthFailed,
                (int)HttpStatusCode.BadRequest => VKAIFailureKind.BadRequest,
                >= (int)HttpStatusCode.InternalServerError => VKAIFailureKind.ServerError,
                _ => VKAIFailureKind.Transport
            },
            HttpRequestException httpEx => httpEx.StatusCode switch
            {
                HttpStatusCode.TooManyRequests => VKAIFailureKind.RateLimited,
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => VKAIFailureKind.AuthFailed,
                HttpStatusCode.BadRequest => VKAIFailureKind.BadRequest,
                >= HttpStatusCode.InternalServerError => VKAIFailureKind.ServerError,
                _ => VKAIFailureKind.Transport
            },
            _ => VKAIFailureKind.ServerError
        };
    }
}
