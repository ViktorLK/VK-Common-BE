using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Psyche Pipeline orchestration.
/// </summary>
public static class VKPipelineErrors
{
    public static readonly VKError EmptyTapestry = new("AI.Psyche.Pipeline.EmptyTapestry", "Tapestry was not assembled by the pipeline stages.");
}
