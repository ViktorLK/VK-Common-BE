using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A non-generic interface to extract fragments from an orchestration pipeline context.
/// </summary>
public interface IVKPromptExtractor
{
    // [CS.03] Async method signature standard returning Result pattern [CS.01]
    Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(VKOrchestrationPipelineContext context, CancellationToken ct);
}
