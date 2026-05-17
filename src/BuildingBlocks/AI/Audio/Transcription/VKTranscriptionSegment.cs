using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a segment of transcribed audio.
/// </summary>
public sealed record VKTranscriptionSegment
{
    /// <summary>
    /// Gets the transcribed text for this segment.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the start time of the segment.
    /// </summary>
    public TimeSpan Start { get; init; }

    /// <summary>
    /// Gets the end time of the segment.
    /// </summary>
    public TimeSpan End { get; init; }

    /// <summary>
    /// Gets the confidence score (0-1).
    /// </summary>
    public float Confidence { get; init; }

    /// <summary>
    /// Gets the speaker identifier if diarization is enabled.
    /// </summary>
    public string? SpeakerId { get; init; }
}
