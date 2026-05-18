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

    /// <summary>
    /// Error returned when the memory key format is invalid.
    /// </summary>
    public static readonly VKError InvalidFormat = new("AI.Memory.InvalidFormat", "The memory key format is invalid.");

    /// <summary>
    /// Error returned when a requested key is not found in the structured memory.
    /// </summary>
    public static readonly VKError KeyNotFound = new("AI.Memory.KeyNotFound", "The requested memory key was not found.");
}
