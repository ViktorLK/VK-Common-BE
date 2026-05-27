using System.Collections.Generic;


namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the comprehensive result of a cognitive pipeline execution.
/// </summary>
public sealed record VKOrchestrationResult
{
    /// <summary>
    /// Gets the final woven prompt tapestry (if produced by weaving stage).
    /// </summary>
    public VKPromptTapestry? Tapestry { get; init; }

    /// <summary>
    /// Gets the final output message or content from the agent (Act stage).
    /// </summary>
    public string? Output { get; init; }


    /// <summary>
    /// Gets the memories retrieved during the process (Recall stage).
    /// </summary>
    public IEnumerable<VKMemoryQueryResult> RecalledMemories { get; init; } = [];


    /// <summary>
    /// Gets the identified user intent.
    /// </summary>
    public VKIntent? Intent { get; init; }

    /// <summary>
    /// Gets the internal "thoughts" or reasoning steps (Think stage).
    /// </summary>
    public string? Reasoning { get; init; }

    /// <summary>
    /// Gets any metadata produced by the pipeline.
    /// </summary>
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
