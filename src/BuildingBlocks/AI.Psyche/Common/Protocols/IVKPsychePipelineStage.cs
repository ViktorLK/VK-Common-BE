using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// A single mental stage in the Psyche orchestrator assembly line.
/// Follows AP.01 and CS.03.
/// </summary>
public interface IVKPsychePipelineStage
{
    /// <summary>
    /// Execution priority. Lower orders execute first.
    /// </summary>
    int StageOrder { get; }

    /// <summary>
    /// Whether this stage is active. Disabled stages are skipped.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Whether this stage should be executed in parallel with other stages in the same group/order block.
    /// </summary>
    bool IsParallel { get; }

    /// <summary>
    /// Optional grouping identifier for parallel execution. Stages with the same non-null value
    /// or the same order execute concurrently.
    /// </summary>
    int? ParallelGroup { get; }

    /// <summary>
    /// Executes the stage's mental logic on the context.
    /// </summary>
    Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default);
}
