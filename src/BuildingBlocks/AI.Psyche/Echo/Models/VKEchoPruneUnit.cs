namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the unit of pruning/truncation for short-term memory conversation history.
/// Follows AP.03 (One file, one type).
/// </summary>
public enum VKEchoPruneUnit
{
    /// <summary>
    /// Prune history by complete conversation turns (e.g. user-assistant pairs).
    /// Prevents dangling single-message context fragmentation.
    /// </summary>
    Turn,

    /// <summary>
    /// Prune history by individual messages.
    /// Maximize token capacity but might split a dialog exchange.
    /// </summary>
    Message
}
