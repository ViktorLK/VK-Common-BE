using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for a Token Budgeter.
/// Manages context window limits and truncation strategies.
/// </summary>
public interface IVKTokenBudgeter
{
    /// <summary>
    /// Calculates the remaining token budget for a conversation.
    /// </summary>
    /// <param name="history">The conversation history.</param>
    /// <param name="maxTokens">The maximum tokens allowed for the request.</param>
    /// <param name="modelId">The target model ID.</param>
    /// <returns>The remaining token count.</returns>
    int GetRemainingBudget(IEnumerable<VKChatMessage> history, int maxTokens, string? modelId = null);

    /// <summary>
    /// Truncates a conversation history to fit within a token budget.
    /// </summary>
    /// <param name="history">The conversation history.</param>
    /// <param name="budget">The target token budget.</param>
    /// <param name="strategy">The truncation strategy (e.g., OldestFirst, Summarization).</param>
    /// <param name="modelId">The target model ID.</param>
    /// <returns>The truncated history.</returns>
    IEnumerable<VKChatMessage> ApplyBudget(
        IEnumerable<VKChatMessage> history,
        int budget,
        VKTokenBudgetStrategy strategy = VKTokenBudgetStrategy.OldestFirst,
        string? modelId = null);
}

/// <summary>
/// Defines strategies for handling token budget overflows.
/// </summary>
public enum VKTokenBudgetStrategy
{
    /// <summary>
    /// Removes the oldest messages first.
    /// </summary>
    OldestFirst,

    /// <summary>
    /// Summarizes the oldest messages.
    /// </summary>
    Summarize,

    /// <summary>
    /// Throws an error if the budget is exceeded.
    /// </summary>
    Error
}
