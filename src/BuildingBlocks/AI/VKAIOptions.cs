using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root options for the AI building block.
/// Acts as the identity anchor for the block.
/// Following BB.01 and BB.06.
/// </summary>
public sealed partial record VKAIOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIBlock.BlockName}";
}
