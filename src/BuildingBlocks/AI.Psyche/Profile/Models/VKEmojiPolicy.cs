namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the emoji usage constraint for generated responses.
/// Follows AP.01 and AP.03.
/// </summary>
public enum VKEmojiPolicy : byte
{
    /// <summary>
    /// Strict prohibition of emojis in output. Recommended for enterprise/engineering contexts.
    /// </summary>
    None = 0,

    /// <summary>
    /// Minimal emoji usage, strictly limited to status indicators or list item markers.
    /// </summary>
    Minimal = 1,

    /// <summary>
    /// Rich and expressive emoji usage for casual, engaging, or social conversations.
    /// </summary>
    Rich = 2
}
