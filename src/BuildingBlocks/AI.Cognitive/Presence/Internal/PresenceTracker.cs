using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Thread-safe implementation of <see cref="IVKPresenceTracker"/> utilizing <see cref="IVKPresenceStateStore"/> for session persistence.
/// Follows CS.01, CS.03, CS.06, AP.01, and OR.01.
/// </summary>
internal sealed class PresenceTracker : IVKPresenceTracker
{
    private readonly TimeProvider _timeProvider; // [CS.06]
    private readonly VKPresenceOptions _presenceOptions;
    private readonly VKFramingOptions _framingOptions;
    private readonly ILogger<PresenceTracker> _logger;
    private readonly IVKPresenceStateStore _stateStore;
    private readonly IVKUserContext _userContext;
    private readonly IVKPresenceQuotaProvider _quotaProvider;
    


    public PresenceTracker(
        TimeProvider timeProvider,
        IOptions<VKPresenceOptions> presenceOptions,
        IOptions<VKFramingOptions> framingOptions,
        ILogger<PresenceTracker> logger,
        IVKPresenceStateStore stateStore,
        IVKUserContext userContext,
        IVKPresenceQuotaProvider quotaProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
        _presenceOptions = VKGuard.NotNull(presenceOptions).Value;
        _framingOptions = VKGuard.NotNull(framingOptions).Value;
        _logger = VKGuard.NotNull(logger);
        _stateStore = VKGuard.NotNull(stateStore);
        _userContext = VKGuard.NotNull(userContext);
        _quotaProvider = VKGuard.NotNull(quotaProvider);
    }

    private VKPresenceStoreKey BuildKey(string sessionId)
    {
        return new VKPresenceStoreKey
        {
            TenantId = _userContext.TenantId ?? "Default",
            UserId = _userContext.UserId ?? "Anonymous",
            SessionId = sessionId
        };
    }

    /// <inheritdoc />
    public async Task<VKResult<VKPresenceState>> CaptureStateAsync(
        string sessionId,
        string? input,
        VKWorldState? worldState = null,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNullOrWhiteSpace(sessionId);

        var now = _timeProvider.GetUtcNow();

        // Load existing state from resumption store, fallback to default if not found
        int promptTokens = 0;
        int completionTokens = 0;
        int turnCount = 0;
        var activeWorldState = VKWorldState.Default;

        var key = BuildKey(sessionId);
        var loadResult = await _stateStore.LoadStateAsync(key, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (loadResult.IsSuccess)
        {
            var existingState = loadResult.Value;



            promptTokens = existingState.TotalPromptTokensUsed;
            completionTokens = existingState.TotalCompletionTokensUsed;
            turnCount = existingState.ActiveMessageCount / 2;
            activeWorldState = existingState.WorldState;
        }

        if (worldState is not null)
        {
            activeWorldState = worldState;
        }
        else if (loadResult.IsSuccess)
        {
            activeWorldState = loadResult.Value.WorldState;
        }



        // Increment dialogue turn count
        turnCount++;

        // Business hours definition: Mon-Fri, 9:00 - 18:00 (Convert using provider local timezone for determinism [CS.06])
        var localTime = TimeZoneInfo.ConvertTime(now, _timeProvider.LocalTimeZone);
        var isBusinessHours = localTime.DayOfWeek != DayOfWeek.Saturday
                              && localTime.DayOfWeek != DayOfWeek.Sunday
                              && localTime.Hour >= 9
                              && localTime.Hour < 18;

        var tenantId = _userContext.TenantId ?? "Default";
        var userId = _userContext.UserId ?? "Anonymous";

        // Fetch dynamic token quota per user/tenant
        var quotaResult = await _quotaProvider.GetQuotaAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        if (quotaResult.IsFailure)
        {
            return VKResult.Failure<VKPresenceState>(quotaResult.Errors);
        }
        var quota = quotaResult.Value;

        var tokenLimit = quota.TokenLimit;
        var thresholdTokens = (int)(tokenLimit * _framingOptions.TruncationThreshold);
        var totalTokensUsed = promptTokens + completionTokens;
        var remainingBudget = Math.Max(0, thresholdTokens - totalTokensUsed);

        var state = new VKPresenceState
        {
            SessionId = sessionId,
            TenantId = tenantId,
            UserId = userId,
            CurrentTime = now,
            IsBusinessHours = isBusinessHours,
            DayOfWeek = now.DayOfWeek,
            TotalPromptTokensUsed = promptTokens,
            TotalCompletionTokensUsed = completionTokens,
            ActiveMessageCount = turnCount * 2,
            RemainingTokenBudget = remainingBudget,
            RecentInput = input,
            WorldState = activeWorldState,
            Environment = _framingOptions.Environment,
            PipelineStage = turnCount <= 1 ? "Initiation" : "Reasoning",
            MaxRequestTokenQuota = quota.MaxRequestTokenQuota,
            SafetyMarginTokens = quota.SafetyMarginTokens
        };

        // Save updated snapshot back to state store for resumption
        var saveResult = await _stateStore.SaveStateAsync(key, state, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (saveResult.IsFailure)
        {
            _logger.PresenceStateSaveFailed(sessionId);
            return VKResult.Failure<VKPresenceState>(VKPresenceErrors.TrackingFailed);
        }

        _logger.PresenceStateCaptured(sessionId, now, totalTokensUsed);

        return VKResult.Success(state);
    }

    /// <inheritdoc />
    public async Task<VKResult> RecordUsageAsync(
        string sessionId,
        int promptTokens,
        int completionTokens,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNullOrWhiteSpace(sessionId);

        if (promptTokens < 0 || completionTokens < 0)
        {
            return VKResult.Failure(VKPresenceErrors.InvalidTokenUsage);
        }

        var tenantId = _userContext.TenantId ?? "Default";
        var userId = _userContext.UserId ?? "Anonymous";

        // Fetch dynamic token quota per user/tenant
        var quotaResult = await _quotaProvider.GetQuotaAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        if (quotaResult.IsFailure)
        {
            return VKResult.Failure(quotaResult.Errors);
        }
        var quota = quotaResult.Value;

        var key = BuildKey(sessionId);
        // Load existing state or initialize new
        var loadResult = await _stateStore.LoadStateAsync(key, cancellationToken).ConfigureAwait(false); // [CS.03]
        VKPresenceState existingState;

        if (loadResult.IsSuccess)
        {
            existingState = loadResult.Value;
        }
        else
        {
            var now = _timeProvider.GetUtcNow();
            existingState = new VKPresenceState
            {
                SessionId = sessionId,
                TenantId = tenantId,
                UserId = userId,
                CurrentTime = now,
                IsBusinessHours = false,
                DayOfWeek = now.DayOfWeek,
                TotalPromptTokensUsed = 0,
                TotalCompletionTokensUsed = 0,
                ActiveMessageCount = 0,
                RemainingTokenBudget = quota.TokenLimit,
                WorldState = VKWorldState.Default,
                Environment = _framingOptions.Environment,
                PipelineStage = "Initiation",
                MaxRequestTokenQuota = quota.MaxRequestTokenQuota,
                SafetyMarginTokens = quota.SafetyMarginTokens
            };
        }

        int newPromptTotal = existingState.TotalPromptTokensUsed + promptTokens;
        int newCompletionTotal = existingState.TotalCompletionTokensUsed + completionTokens;
        int totalTokensUsed = newPromptTotal + newCompletionTotal;
        var tokenLimit = quota.TokenLimit;
        var thresholdTokens = (int)(tokenLimit * _framingOptions.TruncationThreshold);
        var remainingBudget = Math.Max(0, thresholdTokens - totalTokensUsed);

        var updatedState = existingState with
        {
            TotalPromptTokensUsed = newPromptTotal,
            TotalCompletionTokensUsed = newCompletionTotal,
            RemainingTokenBudget = remainingBudget
        };



        var saveResult = await _stateStore.SaveStateAsync(key, updatedState, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (saveResult.IsFailure)
        {
            return VKResult.Failure(VKPresenceErrors.TrackingFailed);
        }

        _logger.TokenUsageRecorded(sessionId, promptTokens, completionTokens);

        return VKResult.Success();
    }

    /// <inheritdoc />
    public async Task<VKResult<VKPresenceState>> GetStateAsync(
        string sessionId,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNullOrWhiteSpace(sessionId);

        var key = BuildKey(sessionId);
        var loadResult = await _stateStore.LoadStateAsync(key, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (loadResult.IsFailure)
        {
            _logger.SessionNotFound(sessionId);
            return VKResult.Failure<VKPresenceState>(VKPresenceErrors.SessionNotFound);
        }

        var stats = loadResult.Value;
        var now = _timeProvider.GetUtcNow();
        // Convert using provider local timezone for determinism [CS.06]
        var localTime = TimeZoneInfo.ConvertTime(now, _timeProvider.LocalTimeZone);
        var isBusinessHours = localTime.DayOfWeek != DayOfWeek.Saturday
                              && localTime.DayOfWeek != DayOfWeek.Sunday
                              && localTime.Hour >= 9
                              && localTime.Hour < 18;

        var tenantId = stats.TenantId ?? _userContext.TenantId ?? "Default";
        var userId = stats.UserId ?? _userContext.UserId ?? "Anonymous";

        // Fetch dynamic token quota per user/tenant
        var quotaResult = await _quotaProvider.GetQuotaAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        if (quotaResult.IsFailure)
        {
            return VKResult.Failure<VKPresenceState>(quotaResult.Errors);
        }
        var quota = quotaResult.Value;

        var tokenLimit = quota.TokenLimit;
        var thresholdTokens = (int)(tokenLimit * _framingOptions.TruncationThreshold);
        var totalTokensUsed = stats.TotalPromptTokensUsed + stats.TotalCompletionTokensUsed;
        var remainingBudget = Math.Max(0, thresholdTokens - totalTokensUsed);

        var state = stats with
        {
            CurrentTime = now,
            IsBusinessHours = isBusinessHours,
            DayOfWeek = now.DayOfWeek,
            RemainingTokenBudget = remainingBudget,
            RecentInput = null,
            MaxRequestTokenQuota = quota.MaxRequestTokenQuota,
            SafetyMarginTokens = quota.SafetyMarginTokens
        };

        return VKResult.Success(state);
    }
}
