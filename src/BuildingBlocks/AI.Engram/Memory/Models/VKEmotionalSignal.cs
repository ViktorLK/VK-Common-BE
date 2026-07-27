namespace VK.Blocks.AI.Engram;

/// <summary>
/// Represents the emotional dimension values (Valence-Arousal model) of a memory.
/// </summary>
public sealed record VKEmotionalSignal
{
    // // [AP.01] sealed record and immutable properties
    /// <summary>
    /// Gets the emotional valence (positivity/negativity) ranging from -1.0 (extreme negative) to 1.0 (extreme positive).
    /// </summary>
    public float Valence { get; init; }

    /// <summary>
    /// Gets the emotional arousal (intensity/excitement) ranging from 0.0 (calm) to 1.0 (intense/highly aroused).
    /// </summary>
    public float Arousal { get; init; }
}
