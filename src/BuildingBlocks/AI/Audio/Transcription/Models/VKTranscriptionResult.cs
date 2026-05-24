using System;
using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the detailed result of an audio transcription request.
/// </summary>
public sealed record VKTranscriptionResult
{
    /// <summary>
    /// Gets the full transcribed text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the individual segments with timestamps.
    /// </summary>
    public IReadOnlyList<VKTranscriptionSegment> Segments { get; init; } = [];

    /// <summary>
    /// Gets the total duration of the audio processed.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the detected language if applicable.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets any additional metadata from the provider.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
