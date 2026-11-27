using System;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Immutable ledger entry representing a single AI invocation's cost and usage.
/// </summary>
public sealed record VKAICostLedgerEntry
{
    public required string InvocationId { get; init; }
    public int AttemptNo { get; init; } = 1;
    public required VKAIScopeKey ScopeKey { get; init; }
    public VKAICorrelationTags Tags { get; init; } = new();
    public required string Provider { get; init; }
    public required string ModelId { get; init; }
    public required VKAICostResult Cost { get; init; }
    public required VKAIUsageMetrics Usage { get; init; }
    public DateTimeOffset RecordedAt { get; init; }
}
