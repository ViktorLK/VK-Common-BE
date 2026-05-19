using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Weaves retrieved knowledge entries into chat messages or system instructions
/// based on their target prompt insertion positions (compatible with SillyTavern World Info).
/// </summary>
public interface IVKKnowledgeWeaver
{
    /// <summary>
    /// Weaves the retrieved knowledge entries into the provided chat history and system instructions.
    /// </summary>
    /// <param name="messages">The active chat history.</param>
    /// <param name="systemInstructions">The active system instructions.</param>
    /// <param name="retrievedEntries">The retrieved knowledge entries to weave.</param>
    /// <returns>A result containing the woven messages list.</returns>
    // // [CS.01] Return Result<T> only, carry structured error objects on failure
    VKResult<IEnumerable<VKChatMessage>> Weave(
        IEnumerable<VKChatMessage> messages,
        string? systemInstructions,
        IEnumerable<VKKnowledgeEntry> retrievedEntries);
}
