namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the operational lifecycle status of a chat session.
/// Follows AP.03 (One file, one type).
/// </summary>
public enum VKSessionStatus
{
    /// <summary>
    /// Active session allowing ongoing dialogue and memory updates.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Archived read-only session. Prompts can still be woven but new messages/edits are blocked.
    /// </summary>
    Archived = 1,

    /// <summary>
    /// Closed session. Triggers final memory consolidation in background workers.
    /// </summary>
    Closed = 2
}
