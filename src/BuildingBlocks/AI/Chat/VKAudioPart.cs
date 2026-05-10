namespace VK.Blocks.AI;

/// <summary>
/// Represents an audio part of a message (used for speech-to-text or embodied intelligence).
/// </summary>
public sealed record VKAudioPart : IVKChatMessagePart
{
    /// <inheritdoc />
    public string PartType => "audio";

    /// <summary>
    /// Gets the URI or Base64 data of the audio.
    /// </summary>
    public required string AudioSource { get; init; }

    /// <summary>
    /// Gets the MIME type of the audio (e.g., "audio/wav", "audio/mpeg").
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets the duration of the audio in seconds (optional).
    /// </summary>
    public double? DurationSeconds { get; init; }
}
