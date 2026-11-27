using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

/// <summary>
/// Default routing policy that delegates candidate resolution to <see cref="IVKAIProviderPool"/> and <see cref="IVKAIRouter"/>
/// and performs incremental candidate selection with capacity leases and token reservation.
/// </summary>
// [AP.01] [ARCH-02] [CS.01] [CS.03] [CS.06]
internal sealed class DefaultAIRoutingPolicy : IVKAIRoutingPolicy
{
    private static readonly FrozenDictionary<VKAIFailureKind, bool> DefaultSwitchableFailures =
        new Dictionary<VKAIFailureKind, bool>
        {
            [VKAIFailureKind.RateLimited] = true,
            [VKAIFailureKind.Transport] = true,
            [VKAIFailureKind.ServerError] = true,
            [VKAIFailureKind.AuthFailed] = true,
            [VKAIFailureKind.BadRequest] = false,
            [VKAIFailureKind.ContentRejected] = false,
            [VKAIFailureKind.Cancelled] = false,
            [VKAIFailureKind.Timeout] = false,
            [VKAIFailureKind.StreamInterrupted] = false
        }.ToFrozenDictionary();

    private readonly IVKAIProviderPool _providerPool;
    private readonly IVKAIRouter _router;
    private readonly IVKAIAffinityPinStore _pinStore;
    private readonly IVKConnectionValidator _connectionValidator;
    private readonly VKRoutingOptions _routingOptions;
    private readonly VKQuotaOptions _quotaOptions;
    private readonly IVKAIModelCatalog _modelCatalog;
    private readonly IVKAICumulativeQuotaStore _cumulativeQuotaStore;
    private readonly IVKAITokenBudgetManager _tokenBudgetManager;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly IVKCircuitBreaker _circuitBreaker;
    private readonly IVKAIRateLimiter _rateLimiter;
    private readonly IVKAIFailurePolicy _failurePolicy;

    public DefaultAIRoutingPolicy(
        IVKAIProviderPool providerPool,
        IVKAIRouter router,
        IVKAIAffinityPinStore pinStore,
        IVKConnectionValidator connectionValidator,
        VKRoutingOptions routingOptions,
        VKQuotaOptions quotaOptions,
        IVKAIModelCatalog modelCatalog,
        IVKAICumulativeQuotaStore cumulativeQuotaStore,
        IVKAITokenBudgetManager tokenBudgetManager,
        IVKGuidGenerator guidGenerator,
        IVKCircuitBreaker circuitBreaker,
        IVKAIRateLimiter rateLimiter,
        IVKAIFailurePolicy failurePolicy)
    {
        _providerPool = VKGuard.NotNull(providerPool);
        _router = VKGuard.NotNull(router);
        _pinStore = VKGuard.NotNull(pinStore);
        _connectionValidator = VKGuard.NotNull(connectionValidator);
        _routingOptions = VKGuard.NotNull(routingOptions);
        _quotaOptions = VKGuard.NotNull(quotaOptions);
        _modelCatalog = VKGuard.NotNull(modelCatalog);
        _cumulativeQuotaStore = VKGuard.NotNull(cumulativeQuotaStore);
        _tokenBudgetManager = VKGuard.NotNull(tokenBudgetManager);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _failurePolicy = VKGuard.NotNull(failurePolicy);
    }

    public bool IsNoOp => false;

    public async ValueTask<VKResult<VKAIRouteDecision>> DecideAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await DecideWithConnectionsAsync(request, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? VKResult.Success(result.Value.Decision)
            : VKResult.Failure<VKAIRouteDecision>(result.Errors);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of VKAIRouteSelection and its lease is transferred to the caller via VKResult.")]
    public async ValueTask<VKResult<VKAIRouteSelection>> SelectCandidateAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(request);

        var decisionResult = await DecideWithConnectionsAsync(request, cancellationToken).ConfigureAwait(false);
        if (decisionResult.IsFailure)
        {
            return VKResult.Failure<VKAIRouteSelection>(decisionResult.Errors);
        }

        var (decision, connectionMap, isDowngraded) = decisionResult.Value;
        if (decision.Candidates.Count == 0)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAISynapseErrors.NoAvailableProvider);
        }

        int maxAttempts = Math.Min(decision.Candidates.Count, 1 + Math.Max(0, decision.MaxFailoverAttempts));
        return await TryAcquireSelectionAsync(
            request,
            decision,
            connectionMap,
            excluded: null,
            failedProvider: null,
            attemptNo: 1,
            maxAttempts: maxAttempts,
            hasDowngraded: isDowngraded,
            cancellationToken).ConfigureAwait(false);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of VKAIRouteSelection and its lease is transferred to the caller via VKResult.")]
    public async ValueTask<VKResult<VKAIRouteSelection>> SelectNextCandidateAsync(
        VKAIFailoverRequest request,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(request);
        VKGuard.NotNull(request.OriginalRequest);
        VKGuard.NotNull(request.Decision);
        VKGuard.NotNull(request.ExcludedConnectionIds);
        VKGuard.NotNull(request.LastFailure);
        cancellationToken.ThrowIfCancellationRequested();

        VKAIInvocationOutcome lastFailure = request.LastFailure;
        if (lastFailure.FirstChunkCommitted)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAIErrors.StreamInterrupted);
        }

        if (lastFailure.HasToolSideEffects ||
            lastFailure.FailureDisposition == VKAIFailureDisposition.OutcomeUnknown)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAIErrors.OutcomeUnknown);
        }

        if (lastFailure.FailureDisposition == VKAIFailureDisposition.TerminalRejected)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable);
        }

        VKAIRouteDecision decision = request.Decision;
        VKAIFailureDirective directive = _failurePolicy.EvaluateOutcome(lastFailure);
        bool policyAllowsSwitch = directive.IsSwitchable &&
            (lastFailure.FailureDisposition == VKAIFailureDisposition.RequestNotAccepted ||
             IsDecisionSwitchable(decision, lastFailure.FailureKind));

        if (!policyAllowsSwitch)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable);
        }

        int maxAttempts = Math.Min(decision.Candidates.Count, 1 + Math.Max(0, decision.MaxFailoverAttempts));
        int nextAttemptNo = request.NextAttemptNo > 0
            ? request.NextAttemptNo
            : Math.Max(2, lastFailure.AttemptNo + 1);

        if (decision.Candidates.Count == 0 || nextAttemptNo > maxAttempts)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAISynapseErrors.NoAvailableProvider);
        }

        HashSet<string> excluded = new(request.ExcludedConnectionIds, StringComparer.OrdinalIgnoreCase)
        {
            lastFailure.ConnectionId
        };

        string? failedProvider = null;
        if (!decision.AllowCrossProviderFailover)
        {
            for (int i = 0; i < decision.Candidates.Count; i++)
            {
                if (string.Equals(decision.Candidates[i].ConnectionId, lastFailure.ConnectionId, StringComparison.OrdinalIgnoreCase))
                {
                    failedProvider = decision.Candidates[i].Provider;
                    break;
                }
            }
        }

        Dictionary<string, VKAIConnection>? connectionMap = null;
        var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
        if (poolResult.IsSuccess)
        {
            connectionMap = BuildConnectionMap(poolResult.Value);
        }

        return await TryAcquireSelectionAsync(
            request.OriginalRequest,
            decision,
            connectionMap,
            excluded,
            failedProvider,
            nextAttemptNo,
            maxAttempts,
            hasDowngraded: true,
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<VKResult<(VKAIRouteDecision Decision, Dictionary<string, VKAIConnection> ConnectionMap, bool IsDowngraded)>> DecideWithConnectionsAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken,
        bool isDowngraded = false,
        bool skipCumulativeQuotaCheck = false)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(request);

        var effectiveRequest = request;
        var profile = ResolveProfile(effectiveRequest.Profile);

        if (!skipCumulativeQuotaCheck &&
            effectiveRequest.ScopeKey is { } scopeKey &&
            (_quotaOptions.MaxCumulativeTokens.HasValue || _quotaOptions.MaxCumulativeCost.HasValue))
        {
            var usage = await _cumulativeQuotaStore.GetUsageAsync(scopeKey, cancellationToken).ConfigureAwait(false);
            if ((_quotaOptions.MaxCumulativeTokens.HasValue && usage.TotalTokens >= _quotaOptions.MaxCumulativeTokens.Value) ||
                (_quotaOptions.MaxCumulativeCost.HasValue && usage.TotalCost >= _quotaOptions.MaxCumulativeCost.Value))
            {
                if (!isDowngraded &&
                    _quotaOptions.ExceededBehavior == VKQuotaExceededBehavior.Downgrade &&
                    TryResolveFallbackProfileName(profile, effectiveRequest.Profile, out var fallbackProfileName))
                {
                    effectiveRequest = effectiveRequest with { Profile = fallbackProfileName };
                    profile = ResolveProfile(fallbackProfileName);
                    isDowngraded = true;
                }
                else
                {
                    return VKResult.Failure<(VKAIRouteDecision, Dictionary<string, VKAIConnection>, bool)>(VKAISynapseErrors.QuotaExceeded);
                }
            }
        }

        var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
        if (poolResult.IsFailure)
        {
            return VKResult.Failure<(VKAIRouteDecision, Dictionary<string, VKAIConnection>, bool)>(poolResult.Errors);
        }

        var connectionMap = BuildConnectionMap(poolResult.Value);

        IEnumerable<VKAIConnection> scopedPool = poolResult.Value;
        if (profile is not null && profile.CandidateConnectionIds.Count > 0)
        {
            var allowedIds = new HashSet<string>(profile.CandidateConnectionIds, StringComparer.OrdinalIgnoreCase);
            scopedPool = scopedPool.Where(c => allowedIds.Contains(c.Id));
        }

        // SYN-04: Filter by enabled, Catalog capability facts, and endpoint validity before router region/health/strategy sorting.
        var enabledPool = scopedPool.Where(c => c.Enabled).ToList();
        var capablePool = new List<VKAIConnection>(enabledPool.Count);
        var validPool = new List<VKAIConnection>(enabledPool.Count);

        for (int i = 0; i < enabledPool.Count; i++)
        {
            var conn = enabledPool[i];
            if (!SatisfiesCatalogCapabilities(effectiveRequest, conn))
            {
                continue;
            }

            capablePool.Add(conn);
            if (_connectionValidator.ValidateEndpoint(conn.Endpoint).IsSuccess)
            {
                validPool.Add(conn);
            }
        }

        if (isDowngraded && enabledPool.Count > 0 && capablePool.Count == 0)
        {
            return VKResult.Failure<(VKAIRouteDecision, Dictionary<string, VKAIConnection>, bool)>(
                VKAISynapseErrors.CapabilityMismatchOnDowngrade);
        }

        var affinityKey = !string.IsNullOrWhiteSpace(effectiveRequest.AffinityKey)
            ? effectiveRequest.AffinityKey
            : effectiveRequest.CorrelationTags?.RunId ?? effectiveRequest.CorrelationTags?.SessionId;

        string? pinnedConnectionId = null;
        if (!string.IsNullOrWhiteSpace(affinityKey))
        {
            pinnedConnectionId = await _pinStore.GetPinnedConnectionAsync(affinityKey, cancellationToken).ConfigureAwait(false);
        }

        var isPinned = !string.IsNullOrWhiteSpace(pinnedConnectionId);
        var routeArgs = new VKAIRouteArgs
        {
            PreferredConnectionId = isPinned ? pinnedConnectionId : effectiveRequest.PreferredConnectionId,
            AffinityKey = affinityKey
        };

        var candidatesResult = await _router.ResolveCandidatesAsync(routeArgs, validPool, cancellationToken).ConfigureAwait(false);
        if (candidatesResult.IsFailure)
        {
            return VKResult.Failure<(VKAIRouteDecision, Dictionary<string, VKAIConnection>, bool)>(candidatesResult.Errors);
        }

        var candidates = new List<VKAIRouteCandidate>(candidatesResult.Value.Count);
        foreach (var conn in candidatesResult.Value)
        {
            if (!conn.Enabled ||
                !SatisfiesCatalogCapabilities(effectiveRequest, conn) ||
                _connectionValidator.ValidateEndpoint(conn.Endpoint).IsFailure)
            {
                continue;
            }

            var provider = conn.Provider ?? VKAIProviderType.OpenAI;
            var modelId = conn.ModelId ?? VKAIModelIds.OpenAI.Gpt6Luna;
            candidates.Add(new VKAIRouteCandidate
            {
                ConnectionId = conn.Id,
                Provider = provider.ToString(),
                ModelId = modelId,
                DeploymentName = conn.DeploymentName,
                CredentialRef = conn.CredentialRef,
                Region = conn.Region,
                Endpoint = Uri.TryCreate(conn.Endpoint, UriKind.Absolute, out var uri) ? uri : null
            });
        }

        if (isDowngraded && candidatesResult.Value.Count > 0 && candidates.Count == 0)
        {
            return VKResult.Failure<(VKAIRouteDecision, Dictionary<string, VKAIConnection>, bool)>(
                VKAISynapseErrors.CapabilityMismatchOnDowngrade);
        }

        var decision = new VKAIRouteDecision
        {
            Candidates = candidates.AsReadOnly(),
            ProfileName = profile?.Name,
            MaxFailoverAttempts = candidates.Count > 1 ? candidates.Count - 1 : 0,
            AllowCrossProviderFailover = profile?.AllowCrossProviderFailover ?? _routingOptions.EnableCrossProviderFallback,
            SwitchableFailures = DefaultSwitchableFailures
        };

        return VKResult.Success((decision, connectionMap, isDowngraded));
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of VKAIRouteSelection and its lease is transferred to the caller via VKResult.")]
    private async ValueTask<VKResult<VKAIRouteSelection>> TryAcquireSelectionAsync(
        VKAIRouteRequest originalRequest,
        VKAIRouteDecision decision,
        Dictionary<string, VKAIConnection>? connectionMap,
        HashSet<string>? excluded,
        string? failedProvider,
        int attemptNo,
        int maxAttempts,
        bool hasDowngraded,
        CancellationToken cancellationToken)
    {
        VKError? lastError = null;

        for (int i = 0; i < decision.Candidates.Count; i++)
        {
            VKAIRouteCandidate candidate = decision.Candidates[i];
            if (excluded is not null && excluded.Contains(candidate.ConnectionId))
            {
                continue;
            }

            if (!decision.AllowCrossProviderFailover &&
                failedProvider is not null &&
                !string.Equals(candidate.Provider, failedProvider, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            VKCircuitHealthSnapshot snapshot = _circuitBreaker.GetSnapshot(candidate.ConnectionId);
            if (snapshot.State is not (VKCircuitState.Closed or VKCircuitState.HalfOpen))
            {
                continue;
            }

            bool acquiredHalfOpenProbe = false;
            if (snapshot.State == VKCircuitState.HalfOpen)
            {
                if (!_circuitBreaker.IsAllowed(candidate.ConnectionId))
                {
                    continue;
                }

                acquiredHalfOpenProbe = true;
            }

            VKAIConnection connection = connectionMap is not null &&
                connectionMap.TryGetValue(candidate.ConnectionId, out var pooled)
                    ? pooled
                    : new VKAIConnection
                    {
                        Id = candidate.ConnectionId,
                        Name = candidate.ConnectionId,
                        ModelId = candidate.ModelId,
                        DeploymentName = candidate.DeploymentName,
                        CredentialRef = candidate.CredentialRef,
                        Endpoint = candidate.Endpoint?.ToString(),
                        Region = candidate.Region
                    };

            string reservationId = _guidGenerator.Create().ToString("N");
            IVKAICapacityLease? lease = null;

            try
            {
                lease = await _rateLimiter
                    .AcquireLeaseAsync(connection, reservationId, cancellationToken)
                    .ConfigureAwait(false);

                if (!lease.IsAcquired)
                {
                    var retryAfter = lease.RetryAfter;
                    lease.Dispose();
                    lease = null;
                    if (acquiredHalfOpenProbe)
                    {
                        _circuitBreaker.ReleaseHalfOpenProbe(candidate.ConnectionId);
                    }

                    lastError = VKAISynapseErrors.CreateRateLimitExceeded(retryAfter);
                    continue;
                }

                if (originalRequest.ScopeKey is { } scopeKey &&
                    originalRequest.EstimatedContextTokens is > 0 and var estimatedTokens)
                {
                    var reserveResult = await _tokenBudgetManager
                        .ReserveTokensAsync(scopeKey, reservationId, estimatedTokens, cancellationToken)
                        .ConfigureAwait(false);

                    if (reserveResult.IsFailure)
                    {
                        // SYN-07: Atomic pre-call cleanup of acquired lease and half-open probe.
                        lease.Dispose();
                        lease = null;
                        if (acquiredHalfOpenProbe)
                        {
                            _circuitBreaker.ReleaseHalfOpenProbe(candidate.ConnectionId);
                        }

                        if (!hasDowngraded &&
                            _quotaOptions.ExceededBehavior == VKQuotaExceededBehavior.Downgrade)
                        {
                            var currentProfileName = decision.ProfileName ?? originalRequest.Profile;
                            var currentProfile = ResolveProfile(currentProfileName);
                            if (TryResolveFallbackProfileName(currentProfile, currentProfileName, out var fallbackProfileName))
                            {
                                var downgradedRequest = originalRequest with { Profile = fallbackProfileName };
                                var downgradedDecisionResult = await DecideWithConnectionsAsync(
                                    downgradedRequest,
                                    cancellationToken,
                                    isDowngraded: true,
                                    skipCumulativeQuotaCheck: true).ConfigureAwait(false);

                                if (downgradedDecisionResult.IsFailure)
                                {
                                    return VKResult.Failure<VKAIRouteSelection>(downgradedDecisionResult.Errors);
                                }

                                var (downgradedDecision, downgradedConnectionMap, _) = downgradedDecisionResult.Value;
                                if (downgradedDecision.Candidates.Count == 0)
                                {
                                    return VKResult.Failure<VKAIRouteSelection>(VKAISynapseErrors.NoAvailableProvider);
                                }

                                int downgradedMaxAttempts = Math.Min(
                                    downgradedDecision.Candidates.Count,
                                    1 + Math.Max(0, downgradedDecision.MaxFailoverAttempts));

                                return await TryAcquireSelectionAsync(
                                    downgradedRequest,
                                    downgradedDecision,
                                    downgradedConnectionMap,
                                    excluded,
                                    failedProvider,
                                    attemptNo,
                                    downgradedMaxAttempts,
                                    hasDowngraded: true,
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }

                        return VKResult.Failure<VKAIRouteSelection>(reserveResult.Errors);
                    }
                }

                return VKResult.Success(new VKAIRouteSelection
                {
                    Candidate = candidate,
                    Lease = lease,
                    Decision = decision,
                    AttemptNo = attemptNo,
                    MaxAttempts = maxAttempts
                });
            }
            catch
            {
                lease?.Dispose();
                if (acquiredHalfOpenProbe)
                {
                    _circuitBreaker.ReleaseHalfOpenProbe(candidate.ConnectionId);
                }

                throw;
            }
        }

        return VKResult.Failure<VKAIRouteSelection>(lastError ?? VKAISynapseErrors.NoAvailableProvider);
    }

    private VKAIProfile? ResolveProfile(string? profileName)
    {
        return !string.IsNullOrWhiteSpace(profileName)
            ? _routingOptions.Profiles.FirstOrDefault(p =>
                string.Equals(p.Name, profileName, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    private bool TryResolveFallbackProfileName(
        VKAIProfile? currentProfile,
        string? currentProfileName,
        [NotNullWhen(true)] out string? fallbackProfileName)
    {
        string? candidate = !string.IsNullOrWhiteSpace(currentProfile?.FallbackProfileName)
            ? currentProfile.FallbackProfileName
            : _quotaOptions.FallbackProfile;

        string? effectiveCurrentName = currentProfile?.Name ?? currentProfileName;
        if (!string.IsNullOrWhiteSpace(candidate) &&
            !string.Equals(candidate, effectiveCurrentName, StringComparison.OrdinalIgnoreCase))
        {
            fallbackProfileName = candidate;
            return true;
        }

        fallbackProfileName = null;
        return false;
    }

    private bool SatisfiesCatalogCapabilities(VKAIRouteRequest request, VKAIConnection connection)
    {
        var provider = connection.Provider ?? VKAIProviderType.OpenAI;
        var modelId = connection.ModelId ?? VKAIModelIds.OpenAI.Gpt6Luna;
        var metadata = _modelCatalog.GetAIModelMetadata(provider, modelId);

        if (metadata.IsDeprecated)
        {
            return false;
        }

        if (request.RequiresTools && metadata.ToolsCapability == VKModelCapabilityState.Unsupported)
        {
            return false;
        }

        if (request.RequiresStructuredOutput && metadata.StructuredOutputCapability == VKModelCapabilityState.Unsupported)
        {
            return false;
        }

        if (request.EstimatedContextTokens.HasValue &&
            metadata.ContextWindowSize > 0 &&
            metadata.ContextWindowSize < request.EstimatedContextTokens.Value)
        {
            return false;
        }

        return true;
    }

    private static Dictionary<string, VKAIConnection> BuildConnectionMap(IReadOnlyList<VKAIConnection> connections)
    {
        var map = new Dictionary<string, VKAIConnection>(connections.Count, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < connections.Count; i++)
        {
            VKAIConnection connection = connections[i];
            if (!string.IsNullOrWhiteSpace(connection.Id))
            {
                map[connection.Id] = connection;
            }
        }

        return map;
    }

    private static bool IsDecisionSwitchable(VKAIRouteDecision decision, VKAIFailureKind failureKind)
    {
        if (decision.SwitchableFailures is not null &&
            decision.SwitchableFailures.TryGetValue(failureKind, out bool isSwitchable))
        {
            return isSwitchable;
        }

        return DefaultSwitchableFailures.TryGetValue(failureKind, out bool defaultSwitchable) && defaultSwitchable;
    }
}
