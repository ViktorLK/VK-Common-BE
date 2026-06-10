using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline stage that runs BEFORE the LLM call in Psyche.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsycheBeforePipelineStage
{
    int StageOrder { get; }
    bool IsActive { get; }
    bool IsParallel { get; }
    int? ParallelGroup { get; }

    Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken);
}
