using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a pipeline stage that runs BEFORE the terminal action.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKBeforePipelineStage<in TContext> where TContext : class
{
    /// <summary>
    /// Gets a value indicating whether this stage is active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the schedule configuration for this stage.
    /// </summary>
    VKPipelineStageSchedule Schedule { get; }

    /// <summary>
    /// Executes the stage logic.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure of this stage.</returns>
    Task<VKResult> ExecuteAsync(TContext context, CancellationToken cancellationToken);
}
