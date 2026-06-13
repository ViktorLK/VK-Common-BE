using System.Collections.Generic;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// The final, immutable output returned from the prompt weaving pipeline.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKPsycheResponse
{
    /// <summary>
    /// Gets the final list of woven chat messages to be sent to the AI model.
    /// </summary>
    public required IReadOnlyList<VKChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets the compiled system instructions or metaprompt, if any.
    /// </summary>
    public string? SystemInstructions { get; init; }

    /// <summary>
    /// Gets the estimated total number of tokens consumed by this tapestry.
    /// </summary>
    public int TotalEstimatedTokens { get; init; }

    /// <summary>
    /// Gets the raw response from the LLM chat engine.
    /// </summary>
    public VKChatResponse? ChatResponse { get; init; }

    /// <summary>
    /// Gets the structured/parsed output processed by the after stages.
    /// </summary>
    public object? ModelResult { get; init; }

    /// <summary>
    /// Gets all active prompt fragments that were successfully woven into the tapestry.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> ActiveFragments { get; init; } = [];

    /// <summary>
    /// Gets any prompt fragments that were evicted or truncated during token management.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> EvictedFragments { get; init; } = [];

    /// <summary>
    /// Gets the execution duration profiling metrics in milliseconds per pipeline stage or task.
    /// </summary>
    public IReadOnlyDictionary<string, double> ProfilingMetrics { get; init; } = new Dictionary<string, double>();

    /// <summary>
    /// Gets extensible metadata key-value pairs generated during pipeline execution.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the actual token usage information for the LLM request.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets the correlation identifier linked with the request pipeline execution.
    /// </summary>
    public string? CorrelationId { get; init; }
}
