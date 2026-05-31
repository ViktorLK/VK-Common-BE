using System;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// A single pulse of conversation history representing an echo in short term memory.
/// Follows AP.01 (sealed record for immutability).
/// </summary>
public sealed record VKEchoTrace : IVKFragmentMetadata
{
    public required VKChatRole Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
