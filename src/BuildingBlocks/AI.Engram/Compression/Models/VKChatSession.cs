using System;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for compressing AI engrams.
/// </summary>
public sealed record VKChatSession
{
    public required VKChatSessionId Id { get; init; }
    public required VKPersonaId PersonaId { get; init; }
    public required string Summary { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
