using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline stage that runs AFTER the LLM call in Psyche.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsycheAfterPipelineStage
{
    VKStageSchedule Schedule { get; }
    bool IsActive { get; }

    Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken);
}
