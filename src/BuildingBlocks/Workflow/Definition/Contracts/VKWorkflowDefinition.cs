using System.Collections.Frozen;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Defines and validates the state transition whitelist table for Workflow lifecycles.
/// Follows AP.01 and CS.01.
/// </summary>
public sealed class VKWorkflowDefinition
{
    private static readonly FrozenDictionary<VKWorkflowState, FrozenSet<VKWorkflowState>> AllowedTransitions =
        new Dictionary<VKWorkflowState, FrozenSet<VKWorkflowState>>
        {
            [VKWorkflowState.Pending] = new[]
            {
                VKWorkflowState.Processing,
                VKWorkflowState.Failed
            }.ToFrozenSet(),

            [VKWorkflowState.Processing] = new[]
            {
                VKWorkflowState.Completed,
                VKWorkflowState.Compensating,
                VKWorkflowState.Suspended,
                VKWorkflowState.Failed,
                VKWorkflowState.TimeoutFailed
            }.ToFrozenSet(),

            [VKWorkflowState.Suspended] = new[]
            {
                VKWorkflowState.Processing,
                VKWorkflowState.Compensating,
                VKWorkflowState.Failed,
                VKWorkflowState.TimeoutFailed
            }.ToFrozenSet(),

            [VKWorkflowState.Compensating] = new[]
            {
                VKWorkflowState.Failed,
                VKWorkflowState.CompensationFailed,
                VKWorkflowState.TimeoutFailed
            }.ToFrozenSet(),

            [VKWorkflowState.Completed] = FrozenSet<VKWorkflowState>.Empty,
            [VKWorkflowState.Failed] = FrozenSet<VKWorkflowState>.Empty,
            [VKWorkflowState.CompensationFailed] = FrozenSet<VKWorkflowState>.Empty,
            [VKWorkflowState.TimeoutFailed] = FrozenSet<VKWorkflowState>.Empty
        }.ToFrozenDictionary();

    /// <summary>
    /// Checks whether transitioning from <paramref name="from"/> to <paramref name="to"/> is permitted.
    /// </summary>
    public static bool CanTransition(VKWorkflowState from, VKWorkflowState to)
    {
        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <summary>
    /// Validates whether transitioning from <paramref name="from"/> to <paramref name="to"/> is permitted, returning a <see cref="VKResult"/>.
    /// </summary>
    public static VKResult ValidateTransition(VKWorkflowState from, VKWorkflowState to)
    {
        if (CanTransition(from, to))
        {
            return VKResult.Success();
        }

        return VKResult.Failure(VKError.Validation(
            "Workflow.InvalidStateTransition",
            $"Invalid state transition from '{from}' to '{to}'."));
    }
}
