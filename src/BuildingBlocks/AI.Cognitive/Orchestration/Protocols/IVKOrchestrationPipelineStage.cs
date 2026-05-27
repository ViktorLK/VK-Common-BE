using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive;

public interface IVKOrchestrationPipelineStage
{
    int Order { get; }
    bool IsActive { get; }
    bool IsParallel { get; }
    int? ParallelGroup { get; }
    Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct);
}
