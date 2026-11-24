using System;

namespace VK.Blocks.AI;

/// <summary>
/// Defines shared architectural constants for the VK.Blocks.AI building block.
/// </summary>
// [AP.03] [BB.01]
public static class VKAIConstants
{
    /// <summary>
    /// The resilience pipeline name used for outbound AI model calls.
    /// </summary>
    public const string AIModelCallPipelineName = "ai.model-call";

    /// <summary>
    /// The default logical turn deadline applied to AI orchestration turns.
    /// </summary>
    public static readonly TimeSpan DefaultTurnDeadline = TimeSpan.FromSeconds(30);
}
