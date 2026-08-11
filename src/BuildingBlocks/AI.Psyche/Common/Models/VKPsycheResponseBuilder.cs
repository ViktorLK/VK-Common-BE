using System;
using System.Collections.Generic;
using VK.Blocks.Core;

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
    /// Builds the final immutable VKPsycheResponse from the accumulated state and context.
    /// </summary>
    /// <param name="context">The execution payload context.</param>
    public VKPsycheResponse Build(VKPsycheContext context)
    {
        VKGuard.NotNull(context);

        var evictedState = context.State<VKPsycheEvictedState>();

        return new VKPsycheResponse
        {
            Messages = [.. Messages],
            TotalEstimatedTokens = TotalEstimatedTokens,
            ChatResponse = ChatResponse,
            ModelResult = ModelResult,
            ActiveFragments = context.Fragments,
            EvictedFragments = evictedState?.Evicted ?? [],
            ProfilingMetrics = new Dictionary<string, double>(ProfilingMetrics),
            Metadata = new Dictionary<string, object>(Metadata),
            Usage = Usage,
            CorrelationId = CorrelationId
        };
    }
}
