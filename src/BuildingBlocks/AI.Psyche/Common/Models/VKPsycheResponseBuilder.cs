using System;
using System.Collections.Generic;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// A mutable builder used to accumulate execution results and construct the final VKPsycheResponse.
/// </summary>
public sealed class VKPsycheResponseBuilder
{
    /// <summary>
    /// Gets the list of woven chat messages to be sent to the AI model.
    /// </summary>
    public List<VKChatMessage> Messages { get; } = [];

    /// <summary>
    /// Gets or sets the compiled system instructions or metaprompt, if any.
    /// </summary>
    public string? SystemInstructions { get; set; }

    /// <summary>
    /// Gets or sets the estimated total number of tokens consumed by this tapestry.
    /// </summary>
    public int TotalEstimatedTokens { get; set; }

    /// <summary>
    /// Gets or sets the raw response from the LLM chat engine.
    /// </summary>
    public VKChatResponse? ChatResponse { get; set; }

    /// <summary>
    /// Gets or sets the structured/parsed output processed by the after stages.
    /// </summary>
    public object? ModelResult { get; set; }

    /// <summary>
    /// Gets the list of active prompt fragments successfully woven into the tapestry.
    /// </summary>
    public List<VKPromptFragment> ActiveFragments { get; } = [];

    /// <summary>
    /// Gets the list of prompt fragments evicted or truncated during token management.
    /// </summary>
    public List<VKPromptFragment> EvictedFragments { get; } = [];

    /// <summary>
    /// Gets the execution duration profiling metrics in milliseconds per pipeline stage or task.
    /// </summary>
    public Dictionary<string, double> ProfilingMetrics { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets extensible metadata key-value pairs generated during pipeline execution.
    /// </summary>
    public Dictionary<string, object> Metadata { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the actual token usage information for the LLM request.
    /// </summary>
    public VKAITokenUsage? Usage { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier linked with the request pipeline execution.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Builds the final immutable VKPsycheResponse from the accumulated state.
    /// </summary>
    public VKPsycheResponse Build()
    {
        return new VKPsycheResponse
        {
            Messages = [.. Messages],
            SystemInstructions = SystemInstructions,
            TotalEstimatedTokens = TotalEstimatedTokens,
            ChatResponse = ChatResponse,
            ModelResult = ModelResult,
            ActiveFragments = [.. ActiveFragments],
            EvictedFragments = [.. EvictedFragments],
            ProfilingMetrics = new Dictionary<string, double>(ProfilingMetrics),
            Metadata = new Dictionary<string, object>(Metadata),
            Usage = Usage,
            CorrelationId = CorrelationId
        };
    }
}
