using System;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Contracts;

public sealed record SessionMessageResponse
{
    public required string Id { get; init; }
    public required string SessionId { get; init; }
    public string? Role { get; init; }
    public string? Content { get; init; }
    public int TokenCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
