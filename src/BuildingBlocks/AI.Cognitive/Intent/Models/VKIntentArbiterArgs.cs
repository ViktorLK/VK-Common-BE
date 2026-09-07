using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Execution parameters for the Intent Arbiter.
/// Encapsulates routing candidates and situational context.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKIntentArbiterArgs
{
    /// <summary>
    /// Gets the list of candidate intents to evaluate.
    /// </summary>
    public IReadOnlyList<VKIntent> Candidates { get; init; } = [];

    /// <summary>
    /// Gets the contextual attributes provided to assist disambiguation.
    /// </summary>
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the default intent to assign if arbitration fails.
    /// </summary>
    public VKIntent DefaultIntent { get; init; } = VKIntent.Chat;
}
