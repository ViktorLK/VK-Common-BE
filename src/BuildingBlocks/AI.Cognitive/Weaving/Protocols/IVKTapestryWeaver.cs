using System.Collections.Generic;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Assembles formatted layers into the final unified message flow based on target depth layouts.
/// </summary>
public interface IVKTapestryWeaver
{
    Task<VKResult<VKPromptTapestry>> WeaveAsync(IReadOnlyList<VKFormattedTier> formatted, VKOrchestrationPipelineContext context);
}
