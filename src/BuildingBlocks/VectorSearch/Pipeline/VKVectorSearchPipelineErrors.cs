using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Predefined errors for the Vector Search pipeline.
/// </summary>
public static class VKVectorSearchPipelineErrors
{
    /// <summary>
    /// Error when pipeline execution is aborted.
    /// </summary>
    public static readonly VKError Aborted = VKError.Failure(
        "VectorSearch.Pipeline.Aborted",
        "Pipeline execution was aborted.");

    /// <summary>
    /// Error when search strategy is not registered.
    /// </summary>
    public static readonly VKError SearchStrategyNotFound = VKError.Failure(
        "VectorSearch.Pipeline.SearchStrategyNotFound",
        "Search strategy was not found in the DI container.");

    /// <summary>
    /// Error when query is too short.
    /// </summary>
    public static readonly VKError QueryTooShort = VKError.Failure(
        "VectorSearch.Pipeline.QueryTooShort",
        "The search query text is too short.");

    /// <summary>
    /// Error when query is too long.
    /// </summary>
    public static readonly VKError QueryTooLong = VKError.Failure(
        "VectorSearch.Pipeline.QueryTooLong",
        "The search query text exceeds the maximum allowed length.");

    /// <summary>
    /// Error when query violates security checks.
    /// </summary>
    public static readonly VKError QuerySecurityViolation = VKError.Failure(
        "VectorSearch.Pipeline.QuerySecurityViolation",
        "The search query violated security policies (e.g. SQL injection or prompt injection).");
}
