using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a voice available for speech generation.
/// </summary>
public sealed record VKAudioVoice
{
    /// <summary>
    /// Gets the unique identifier of the voice.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name of the voice.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the gender of the voice if known.
    /// </summary>
    public string? Gender { get; init; }

    /// <summary>
    /// Gets the locale/language of the voice (e.g., "en-US").
    /// </summary>
    public string? Locale { get; init; }

    /// <summary>
    /// Gets any additional metadata for the voice.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
