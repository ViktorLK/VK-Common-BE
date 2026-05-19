using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Helper class for managing and manipulating chat history collections.
/// </summary>
internal static class ChatHistoryHelper
{
    /// <summary>
    /// Trims the chat history to stay within a maximum number of messages,
    /// preserving the system prompt if one exists.
    /// </summary>
    /// <param name="history">The chat history list to trim.</param>
    /// <param name="maxMessages">The maximum number of messages to keep.</param>
    public static void TrimHistory(List<VKChatMessage> history, int maxMessages)
    {
        VKGuard.NotNull(history);

        if (history.Count <= maxMessages)
        {
            return;
        }

        // Keep the System Prompt (usually at index 0)
        var systemPrompt = history.FirstOrDefault(m => m.Role == VKChatRole.System);
        int startIndex = systemPrompt is not null ? 1 : 0;
        int removeCount = history.Count - maxMessages;

        if (removeCount > 0)
        {
            history.RemoveRange(startIndex, Math.Min(removeCount, history.Count - startIndex));
        }
    }
}
