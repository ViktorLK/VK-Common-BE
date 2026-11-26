using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Resilience.Internal;

/// <summary>
/// Default AI failure classification policy mapping HTTP status codes, failure kinds, and dispositions to routing and health directives.
/// </summary>
// [AP.01]
internal sealed class DefaultAIFailurePolicy : IVKAIFailurePolicy
{
    public VKAIFailureAction Evaluate(VKAIFailureKind failureKind)
    {
        return failureKind switch
        {
            VKAIFailureKind.Transport => VKAIFailureAction.Switch,
            VKAIFailureKind.ServerError => VKAIFailureAction.Switch,
            VKAIFailureKind.RateLimited => VKAIFailureAction.Switch,
            VKAIFailureKind.AuthFailed => VKAIFailureAction.DisableConnection,
            VKAIFailureKind.BadRequest => VKAIFailureAction.Stop,
            VKAIFailureKind.ContentRejected => VKAIFailureAction.Stop,
            VKAIFailureKind.Cancelled => VKAIFailureAction.Stop,
            VKAIFailureKind.Timeout => VKAIFailureAction.Stop,
            VKAIFailureKind.StreamInterrupted => VKAIFailureAction.Stop,
            _ => VKAIFailureAction.Stop
        };
    }

    public VKAIFailureDirective EvaluateOutcome(VKAIInvocationOutcome outcome)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(outcome);

        if (outcome.IsSuccess)
        {
            return new VKAIFailureDirective(
                VKAIFailureAction.Stop,
                VKAIHealthAction.RecordSuccess,
                IsSwitchable: false,
                UnpinAffinity: false);
        }

        if (outcome.FailureKind == VKAIFailureKind.Cancelled)
        {
            return new VKAIFailureDirective(
                VKAIFailureAction.Stop,
                VKAIHealthAction.None,
                IsSwitchable: false,
                UnpinAffinity: false);
        }

        if (outcome.HttpStatusCode == 429 || outcome.FailureKind == VKAIFailureKind.RateLimited)
        {
            bool canSwitch = !outcome.FirstChunkCommitted
                && !outcome.HasToolSideEffects
                && outcome.FailureDisposition != VKAIFailureDisposition.TerminalRejected;

            return new VKAIFailureDirective(
                canSwitch ? VKAIFailureAction.Switch : VKAIFailureAction.Stop,
                VKAIHealthAction.RecordCooldown,
                IsSwitchable: canSwitch,
                UnpinAffinity: true);
        }

        if (outcome.HttpStatusCode is 401 or 403 || outcome.FailureKind == VKAIFailureKind.AuthFailed)
        {
            bool canSwitch = !outcome.FirstChunkCommitted
                && !outcome.HasToolSideEffects
                && outcome.FailureDisposition != VKAIFailureDisposition.TerminalRejected;

            return new VKAIFailureDirective(
                canSwitch ? VKAIFailureAction.DisableConnection : VKAIFailureAction.Stop,
                VKAIHealthAction.Isolate,
                IsSwitchable: canSwitch,
                UnpinAffinity: true);
        }

        if (outcome.HttpStatusCode == 400 || outcome.FailureKind is VKAIFailureKind.BadRequest or VKAIFailureKind.ContentRejected)
        {
            return new VKAIFailureDirective(
                VKAIFailureAction.Stop,
                VKAIHealthAction.None,
                IsSwitchable: false,
                UnpinAffinity: false);
        }

        if (outcome.HttpStatusCode >= 500 || outcome.FailureKind is VKAIFailureKind.ServerError or VKAIFailureKind.Transport)
        {
            bool stop = outcome.FirstChunkCommitted
                || outcome.HasToolSideEffects
                || outcome.FailureDisposition is VKAIFailureDisposition.OutcomeUnknown or VKAIFailureDisposition.TerminalRejected;

            return new VKAIFailureDirective(
                stop ? VKAIFailureAction.Stop : VKAIFailureAction.Switch,
                VKAIHealthAction.RecordFailure,
                IsSwitchable: !stop,
                UnpinAffinity: true);
        }

        if (outcome.HttpStatusCode == 408 || outcome.FailureKind is VKAIFailureKind.Timeout or VKAIFailureKind.StreamInterrupted)
        {
            bool stop = outcome.FirstChunkCommitted
                || outcome.HasToolSideEffects
                || outcome.FailureDisposition is VKAIFailureDisposition.OutcomeUnknown or VKAIFailureDisposition.TerminalRejected;

            return new VKAIFailureDirective(
                stop ? VKAIFailureAction.Stop : VKAIFailureAction.Switch,
                VKAIHealthAction.RecordFailure,
                IsSwitchable: !stop,
                UnpinAffinity: true);
        }

        if (outcome.HttpStatusCode is >= 400 and < 500)
        {
            return new VKAIFailureDirective(
                VKAIFailureAction.Stop,
                VKAIHealthAction.None,
                IsSwitchable: false,
                UnpinAffinity: false);
        }

        return new VKAIFailureDirective(
            VKAIFailureAction.Stop,
            VKAIHealthAction.None,
            IsSwitchable: false,
            UnpinAffinity: false);
    }
}
