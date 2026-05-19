namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Basic implementation of a presence signal.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
public sealed record VKPresenceSignal : IVKPresenceSignal
{
    /// <inheritdoc />
    public required double Intensity { get; init; }

    /// <inheritdoc />
    public required double Polarity { get; init; }

    /// <inheritdoc />
    public required double Valence { get; init; }

    /// <inheritdoc />
    public required double Weight { get; init; }
}
