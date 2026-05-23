using System.Collections.Generic;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for estimating token consumption in text and message history.
/// Follows AP.03 (public scope, VK prefix) and CS.01.
/// </summary>
public interface IVKTokenMeter
{
    /// <summary>
    /// Estimates the token count for a raw text string.
    /// </summary>
    /// <param name="text">The target text.</param>
    /// <returns>The estimated token count.</returns>
    int CountTokens(string text);

    /// <summary>
    /// Estimates the token count for a sequence of chat messages, accounting for system templates and formatting.
    /// </summary>
    /// <param name="messages">The sequence of chat messages.</param>
    /// <returns>The estimated token count.</returns>
    int CountTokens(IEnumerable<VKChatMessage> messages);
}
