using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Implements competitive and exclusion logic between overlapping scored fragments.
/// </summary>
public interface IVKPromptPruner
{
    VKResult<IReadOnlyList<VKScoredFragment>> Prune(IReadOnlyList<VKScoredFragment> scored, VKWeavingContext context);
}
