using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an individual task in the prompt weaving pipeline.
/// </summary>
public interface IVKWeavingTask
{
    /// <summary>
    /// Gets the execution order of this task. Lower numbers execute first.
    /// </summary>
    int TaskOrder { get; }

    /// <summary>
    /// Gets a value indicating whether this task can be executed in parallel with other tasks in the same order.
    /// </summary>
    bool IsParallel { get; }

    /// <summary>
    /// Optional grouping key for parallel tasks.
    /// </summary>
    int? ParallelGroup { get; }

    /// <summary>
    /// Executes the weaving task.
    /// </summary>
    Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default);
}
