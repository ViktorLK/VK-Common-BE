using System.Collections.Generic;
using System.IO;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the result of a speech generation request.
/// </summary>
public sealed record VKSpeechResult
{
    /// <summary>
    /// Gets the audio stream.
    /// </summary>
    public required Stream Stream { get; init; }

    /// <summary>
    /// Gets the number of characters synthesized.
    /// </summary>
    public int CharacterCount { get; init; }

    /// <summary>
    /// Gets any additional metadata from the provider.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
