using System.Linq;
using Microsoft.SemanticKernel.ChatCompletion;

namespace VK.Blocks.AI.SemanticKernel.Common.Governance;

/// <summary>
/// Utility for pruning chat context to fit within token limits.
/// </summary>
public sealed class VKContextPruner
{
    /// <summary>
    /// Prunes the chat history to the specified item limit, keeping the system message.
    /// </summary>
    public static void Prune(ChatHistory history, int maxItems)
    {
        if (history.Count <= maxItems)
        {
            return;
        }

        // Keep system message if it is the first message
        var hasSystemMessage = history.Count > 0 && history[0].Role == AuthorRole.System;
        var systemMessage = hasSystemMessage ? history[0] : null;

        // Determine how many messages to keep from the end
        var keepCount = maxItems - (hasSystemMessage ? 1 : 0);
        var messagesToKeep = history.Skip(history.Count - keepCount).ToList();

        history.Clear();

        if (systemMessage is not null)
        {
            history.Add(systemMessage);
        }

        foreach (var message in messagesToKeep)
        {
            history.Add(message);
        }
    }

    /// <summary>
    /// Prunes the chat history precisely based on token count, preserving the system message.
    /// </summary>
    public static void PruneTokens(ChatHistory history, int maxTokens, string? modelId = null)
    {
        if (history.Count == 0)
        {
            return;
        }

        // Keep system message if it is the first message
        var hasSystemMessage = history[0].Role == AuthorRole.System;
        var systemMessage = hasSystemMessage ? history[0] : null;

        // Calculate initial total tokens
        var totalTokens = CalculateHistoryTokens(history, modelId);
        if (totalTokens <= maxTokens)
        {
            return;
        }

        // Iteratively remove oldest messages (excluding the system message at index 0)
        while (totalTokens > maxTokens && history.Count > (hasSystemMessage ? 1 : 0))
        {
            // Remove the second message (since index 0 is the system message) or the first message
            var removeIndex = hasSystemMessage ? 1 : 0;
            history.RemoveAt(removeIndex);
            totalTokens = CalculateHistoryTokens(history, modelId);
        }
    }

    private static int CalculateHistoryTokens(ChatHistory history, string? modelId)
    {
        int total = 0;
        foreach (var message in history)
        {
            if (message.Content is not null)
            {
                total += VKTokenCounter.CountTokens(message.Content, modelId);
            }
        }
        return total;
    }
}
