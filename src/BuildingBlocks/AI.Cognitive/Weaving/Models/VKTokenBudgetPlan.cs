using System;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Strict, fine-grained token limits calculated for the request.
/// Includes support for resolving token meters at runtime.
/// Follows AP.01 (Sealed Record with required properties).
/// </summary>
public sealed record VKTokenBudgetPlan
{
    public required int TotalContextLimit { get; init; }
    public required int MaxResponseTokens { get; init; }
    public required int ReservedSystemTokens { get; init; }
    public required int AvailableHistoryLimit { get; init; }
    public required int AvailableKnowledgeLimit { get; init; }

    /// <summary>
    /// Function to retrieve or use a specific token meter implementation if available.
    /// Retains extensibility at initialization or injection per user feedback.
    /// </summary>
    public Func<IVKTokenMeter>? TokenMeterResolver { get; init; }
}
