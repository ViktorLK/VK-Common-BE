using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for decomposing a high-level goal into a sequence of executable reasoning steps.
/// </summary>
public interface IVKReasoningPlanner
{
    /// <summary>
    /// Creates a reasoning plan to achieve the specified goal.
    /// </summary>
    /// <param name="goal">The high-level goal description.</param>
    /// <param name="args">The reasoning arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the generated goal and steps.</returns>
    Task<VKResult<VKGoal>> PlanAsync(
        string goal,
        VKReasoningArgs? args = null,
        CancellationToken cancellationToken = default);
}
