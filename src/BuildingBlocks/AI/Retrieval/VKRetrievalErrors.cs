using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Retrieval feature.
/// </summary>
public static class VKRetrievalErrors
{
    /// <summary>
    /// Error returned when the retrieval search fails.
    /// </summary>
    public static readonly VKError SearchFailed = new("AI.Retrieval.SearchFailed", "The retrieval search failed.");

    /// <summary>
    /// Error returned when the retrieval feature is disabled in configuration.
    /// </summary>
    public static readonly VKError FeatureDisabled = new("AI.Retrieval.FeatureDisabled", "The retrieval feature is disabled.");
}
