using System;
using VK.Blocks.AI.Agents.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Agents feature.
/// </summary>
public sealed record VKAgentOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Agents options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{AgentsConstants.FeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Agents feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum number of iterations for an agent.
    /// </summary>
    public int MaxIterations { get; init; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of tool calls per iteration.
    /// </summary>
    public int MaxToolCallsPerIteration { get; init; } = 5;

    /// <summary>
    /// Gets or sets the execution timeout for the entire agent task.
    /// </summary>
    public TimeSpan ExecutionTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum total tokens allowed for the entire agentic run.
    /// Optional.
    /// </summary>
    public int? MaxTotalTokens { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow parallel tool calls.
    /// Defaults to true.
    /// </summary>
    public bool AllowParallelToolCalls { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log detailed tool arguments and results.
    /// **WARNING**: This may include PII. Defaults to false.
    /// </summary>
    public bool LogToolData { get; init; } = false;

    /// <summary>
    /// Gets or sets the number of retries for tool execution failures.
    /// Defaults to 3.
    /// </summary>
    public int ToolRetryCount { get; init; } = 3;
}
