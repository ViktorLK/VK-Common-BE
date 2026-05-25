using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Converts raw truncated fragments into target model dialect specifications.
/// </summary>
public interface IVKPromptFormatter<TModel>
{
    VKResult<IReadOnlyList<VKFormattedTier>> Format(IReadOnlyList<VKScoredFragment> truncated, VKWeavingContext context);
}
