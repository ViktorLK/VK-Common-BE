using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// Builder for AISK ChatHistory.
/// </summary>
internal static class AISKChatHistoryBuilder
{
    /// <summary>
    /// Builds a AISK <see cref="ChatHistory"/> from VK chat messages.
    /// </summary>
    /// <param name="messages">The messages.</param>
    /// <returns>The chat history.</returns>
    internal static ChatHistory Build(IEnumerable<VKChatMessage> messages)
    {
        ChatHistory history = [];
        foreach (VKChatMessage message in messages)
        {
            AuthorRole role = message.Role switch
            {
                VKChatRole.System => AuthorRole.System,
                VKChatRole.User => AuthorRole.User,
                VKChatRole.Assistant => AuthorRole.Assistant,
                _ => AuthorRole.User
            };
            history.AddMessage(role, message.Content);
        }
        return history;
    }
}
