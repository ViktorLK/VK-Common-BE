using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Retrieval feature.
/// </summary>
public static class VKRetrievalErrors
{
    /// <summary>
    /// Error returned when the vector store is not available.
    /// </summary>
    public static readonly VKError StoreUnavailable = new("AI.Retrieval.StoreUnavailable", "The vector store is not available.");

    /// <summary>
    /// Error returned when the document loading fails.
    /// </summary>
    public static readonly VKError LoadingFailed = new("AI.Retrieval.LoadingFailed", "The document loading failed.");

    /// <summary>
    /// Error returned when the retrieval feature is disabled.
    /// </summary>
    public static readonly VKError FeatureDisabled = new("AI.Retrieval.Disabled", "The retrieval feature is disabled.");
}
