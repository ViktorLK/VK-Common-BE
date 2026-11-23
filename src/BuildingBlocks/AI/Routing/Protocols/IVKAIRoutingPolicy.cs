using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Routing.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a policy that resolves candidate AI connections for a routing request.
/// </summary>
// [ARCH-02] [CS.01] [CS.03]
public interface IVKAIRoutingPolicy
{
    /// <summary>
    /// Gets a value indicating whether this policy is a no-op fallback implementation.
    /// </summary>
    bool IsNoOp => false;

    /// <summary>
    /// Decides the ordered list of candidate AI connections for the given request.
    /// </summary>
    /// <param name="request">The routing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the routing decision.</returns>
    ValueTask<VKResult<VKAIRouteDecision>> DecideAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects the initial candidate AI connection and acquires an associated capacity lease.
    /// </summary>
    /// <param name="request">The routing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the selected candidate and capacity lease.</returns>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of VKAIRouteSelection and its lease is transferred to the caller via VKResult.")]
    async ValueTask<VKResult<VKAIRouteSelection>> SelectCandidateAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        // [AP.01]
        VKGuard.NotNull(request);

        VKResult<VKAIRouteDecision> decisionResult = await DecideAsync(request, cancellationToken).ConfigureAwait(false);
        if (decisionResult.IsFailure)
        {
            return VKResult.Failure<VKAIRouteSelection>(decisionResult.Errors);
        }

        VKAIRouteDecision decision = decisionResult.Value;
        if (decision.Candidates.Count == 0)
        {
            return VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable);
        }

        VKAIRouteCandidate candidate = decision.Candidates[0];
        int maxAttempts = Math.Min(decision.Candidates.Count, 1 + Math.Max(0, decision.MaxFailoverAttempts));

        return VKResult.Success(new VKAIRouteSelection
        {
            Candidate = candidate,
            Lease = new NoOpCapacityLease(candidate.ConnectionId),
            Decision = decision,
            AttemptNo = 1,
            MaxAttempts = maxAttempts
        });
    }

    /// <summary>
    /// Selects the next eligible candidate AI connection for a failover attempt.
    /// </summary>
    /// <param name="request">The failover request containing prior decision, exclusions, and failure outcome.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the next candidate and capacity lease, or failure when failover is disallowed.</returns>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of VKAIRouteSelection and its lease is transferred to the caller via VKResult.")]
    ValueTask<VKResult<VKAIRouteSelection>> SelectNextCandidateAsync(
        VKAIFailoverRequest request,
        CancellationToken cancellationToken = default)
    {
        // [AP.01]
        VKGuard.NotNull(request);
        VKGuard.NotNull(request.OriginalRequest);
        VKGuard.NotNull(request.Decision);
        VKGuard.NotNull(request.ExcludedConnectionIds);
        VKGuard.NotNull(request.LastFailure);
        cancellationToken.ThrowIfCancellationRequested();

        VKAIInvocationOutcome lastFailure = request.LastFailure;
        if (lastFailure.FirstChunkCommitted)
        {
            return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.StreamInterrupted));
        }

        if (lastFailure.HasToolSideEffects ||
            lastFailure.FailureDisposition == VKAIFailureDisposition.OutcomeUnknown)
        {
            return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.OutcomeUnknown));
        }

        if (lastFailure.FailureDisposition == VKAIFailureDisposition.TerminalRejected)
        {
            return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable));
        }

        VKAIRouteDecision decision = request.Decision;
        if (decision.Candidates.Count == 0 || !IsSwitchable(decision, lastFailure.FailureKind))
        {
            return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable));
        }

        int maxAttempts = Math.Min(decision.Candidates.Count, 1 + Math.Max(0, decision.MaxFailoverAttempts));
        int nextAttemptNo = request.NextAttemptNo > 0
            ? request.NextAttemptNo
            : Math.Max(2, lastFailure.AttemptNo + 1);

        if (nextAttemptNo > maxAttempts)
        {
            return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable));
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

        for (int i = 0; i < decision.Candidates.Count; i++)
        {
            VKAIRouteCandidate candidate = decision.Candidates[i];
            if (excluded.Contains(candidate.ConnectionId))
            {
                continue;
            }

            if (!decision.AllowCrossProviderFailover &&
                failedProvider is not null &&
                !string.Equals(candidate.Provider, failedProvider, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return ValueTask.FromResult(VKResult.Success(new VKAIRouteSelection
            {
                Candidate = candidate,
                Lease = new NoOpCapacityLease(candidate.ConnectionId),
                Decision = decision,
                AttemptNo = nextAttemptNo,
                MaxAttempts = maxAttempts
            }));
        }

        return ValueTask.FromResult(VKResult.Failure<VKAIRouteSelection>(VKAIErrors.ServiceUnavailable));
    }

    private static bool IsSwitchable(VKAIRouteDecision decision, VKAIFailureKind failureKind)
    {
        if (decision.SwitchableFailures is not null &&
            decision.SwitchableFailures.TryGetValue(failureKind, out bool isSwitchable))
        {
            return isSwitchable;
        }

        return failureKind is not (
            VKAIFailureKind.BadRequest
            or VKAIFailureKind.ContentRejected
            or VKAIFailureKind.Cancelled
            or VKAIFailureKind.StreamInterrupted);
    }
}
