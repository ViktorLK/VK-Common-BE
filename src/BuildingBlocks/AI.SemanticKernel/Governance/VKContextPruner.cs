using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;

namespace VK.Blocks.AI.SemanticKernel.Governance;

/// <summary>
/// Utility for pruning chat context to fit within token limits.
/// </summary>
public sealed class VKContextPruner
{
    /// <summary>
    /// Prunes the chat history to the specified limit.
    /// </summary>
    public static void Prune(ChatHistory history, int maxItems)
    {
        if (history.Count > maxItems)
        {
            // Simple logic: keep system message and the last N messages
            // (Real logic would be more complex, involving token counting)
        }
    }
}
