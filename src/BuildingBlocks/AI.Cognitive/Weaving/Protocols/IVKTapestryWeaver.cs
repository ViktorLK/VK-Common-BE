using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Assembles formatted layers into the final unified message flow based on target depth layouts.
/// </summary>
public interface IVKTapestryWeaver
{
    VKResult<VKPromptTapestry> Weave(IReadOnlyList<VKFormattedTier> formatted, VKWeavingContext context);
}
