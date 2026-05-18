namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the sensory presence and environmental signal contract.
/// Metaphor: A single neural impulse of the "Now".
/// Follows AP.03 public naming/location rules.
/// </summary>
public interface IVKPresenceSignal
{
    /// <summary>
    /// Gets the intensity score (0.0 to 1.0) representing magnitude.
    /// </summary>
    double Intensity { get; }

    /// <summary>
    /// Gets the polarity score (-1.0 to 1.0) representing charge direction.
    /// </summary>
    double Polarity { get; }

    /// <summary>
    /// Gets the valence (emotional value score).
    /// </summary>
    double Valence { get; }

    /// <summary>
    /// Gets the signal priority weight.
    /// </summary>
    double Weight { get; }
}
