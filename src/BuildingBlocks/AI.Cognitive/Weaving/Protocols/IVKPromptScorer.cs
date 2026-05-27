using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Calculates scores/weights for the gathered fragments.
/// </summary>
public interface IVKPromptScorer
{
    VKResult<IReadOnlyList<VKScoredFragment>> Score(IReadOnlyList<VKPromptFragment> fragments, VKOrchestrationPipelineContext context);
}
