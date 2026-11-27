using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Defines the routing action to take when an AI provider invocation fails.
/// </summary>
public enum VKAIFailureAction : byte
{
    /// <summary>
    /// Switch to another candidate connection.
    /// </summary>
    Switch = 0,

    /// <summary>
    /// Disable the failing connection and switch to another candidate.
    /// </summary>
    DisableConnection = 1,

    /// <summary>
    /// Stop retrying and fail the invocation immediately.
    /// </summary>
    Stop = 2
}

/// <summary>
/// Defines the circuit-breaker health mutation to apply for an AI invocation outcome.
/// </summary>
public enum VKAIHealthAction : byte
{
    /// <summary>
    /// Do not alter connection health state.
    /// </summary>
    None = 0,

    /// <summary>
    /// Record a successful execution on the connection circuit breaker.
    /// </summary>
    RecordSuccess = 1,

    /// <summary>
    /// Record a failure execution on the connection circuit breaker.
    /// </summary>
    RecordFailure = 2,

    /// <summary>
    /// Place the connection into rate-limit cooldown.
    /// </summary>
    RecordCooldown = 3,

    /// <summary>
    /// Isolate the connection until reconfigured or reset.
    /// </summary>
    Isolate = 4
}

/// <summary>
/// Represents the combined routing, health, failover, and affinity directive evaluated from an AI invocation outcome.
/// </summary>
// [AP.01]
public sealed record VKAIFailureDirective(
    VKAIFailureAction RoutingAction,
    VKAIHealthAction HealthAction,
    bool IsSwitchable,
    bool UnpinAffinity);

/// <summary>
/// Policy that maps an AI invocation failure kind or full invocation outcome to recovery and health actions.
/// </summary>
public interface IVKAIFailurePolicy
{
    /// <summary>
    /// Determines the recovery action for the specified failure kind.
    /// </summary>
    /// <param name="failureKind">The categorized failure kind.</param>
    /// <returns>The action to take.</returns>
    VKAIFailureAction Evaluate(VKAIFailureKind failureKind);

    /// <summary>
    /// Evaluates a full invocation outcome to determine routing, health, failover switchability, and affinity directives.
    /// </summary>
    /// <param name="outcome">The observed invocation outcome.</param>
    /// <returns>The directive describing how routing, health, and affinity state should react.</returns>
    VKAIFailureDirective EvaluateOutcome(VKAIInvocationOutcome outcome)
    {
        // [AP.01]
        VKGuard.NotNull(outcome);

        if (outcome.IsSuccess)
        {
            return new VKAIFailureDirective(
                VKAIFailureAction.Stop,
                VKAIHealthAction.RecordSuccess,
                IsSwitchable: false,
                UnpinAffinity: false);
        }

        var routingAction = Evaluate(outcome.FailureKind);
        var healthAction = routingAction == VKAIFailureAction.DisableConnection
            ? VKAIHealthAction.Isolate
            : outcome.FailureKind == VKAIFailureKind.RateLimited
                ? VKAIHealthAction.RecordCooldown
                : routingAction == VKAIFailureAction.Switch
                    ? VKAIHealthAction.RecordFailure
                    : VKAIHealthAction.None;

        bool isSwitchable = !outcome.FirstChunkCommitted
            && !outcome.HasToolSideEffects
            && outcome.FailureDisposition != VKAIFailureDisposition.OutcomeUnknown
            && outcome.FailureDisposition != VKAIFailureDisposition.TerminalRejected
            && routingAction != VKAIFailureAction.Stop;

        return new VKAIFailureDirective(
            routingAction,
            healthAction,
            IsSwitchable: isSwitchable,
            UnpinAffinity: routingAction != VKAIFailureAction.Stop);
    }
}
