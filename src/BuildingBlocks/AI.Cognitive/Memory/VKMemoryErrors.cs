using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Standard error constants for the Memory feature.
/// </summary>
public static class VKMemoryErrors
{
    /// <summary>
    /// Error returned when the memory store is not available.
    /// </summary>
    public static readonly VKError StoreUnavailable = new("AI.Memory.StoreUnavailable", "The memory store is not available.");

    /// <summary>
    /// Error returned when the summarization fails.
    /// </summary>
    public static readonly VKError SummarizationFailed = new("AI.Memory.SummarizationFailed", "The summarization failed.");
}
