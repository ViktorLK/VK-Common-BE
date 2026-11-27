using System;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Metric details capturing token usage and latency for an AI invocation.
/// </summary>
public sealed record VKAIUsageMetrics
{
    public long PromptTokens { get; init; }
    public long CompletionTokens { get; init; }
    public long CachedTokens { get; init; }
    public long ReasoningTokens { get; init; }
    public long TotalTokens => PromptTokens + CompletionTokens;
    public TimeSpan Duration { get; init; }
    public decimal EstimatedCost { get; init; }
}
