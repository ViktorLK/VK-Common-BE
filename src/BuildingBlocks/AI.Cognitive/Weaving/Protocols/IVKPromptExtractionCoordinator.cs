using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Coordinates multiple <see cref="IVKPromptExtractor"/> instances to aggregate fragments.
/// </summary>
public interface IVKPromptExtractionCoordinator
{
    Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAllAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken cancellationToken = default);
}
