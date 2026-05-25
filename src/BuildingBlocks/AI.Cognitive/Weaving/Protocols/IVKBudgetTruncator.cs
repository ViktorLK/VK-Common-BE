using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Applies knapsack-based budget slicing to keep the fragments within limits.
/// </summary>
public interface IVKBudgetTruncator
{
    VKResult<IReadOnlyList<VKScoredFragment>> Truncate(IReadOnlyList<VKScoredFragment> pruned, VKWeavingContext context);
}
