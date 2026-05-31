namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines Echo settings that can be overridden at the request level.
/// </summary>
public interface IVKEchoOverrides
{
    /// <summary>
    /// Gets the maximum number of items to retain in the sliding window.
    /// </summary>
    int? MaxWindowSize { get; init; }

    /// <summary>
    /// Gets the maximum number of complete conversation turns to retain.
    /// </summary>
    int? MaxTurns { get; init; }
}
