using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Execution context for atomic tools, independent of higher-level agent orchestration (ARCH-01).
/// </summary>
public sealed record VKAtomicToolContext
{
    public string? CorrelationId { get; init; }

    public string? Caller { get; init; }

    public Dictionary<string, object> Variables { get; init; } = [];

    public List<VKAtomicToolResult> ToolCallHistory { get; init; } = [];
}

