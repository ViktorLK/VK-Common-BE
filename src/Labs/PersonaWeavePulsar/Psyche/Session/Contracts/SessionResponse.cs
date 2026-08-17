using System;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Contracts;

public sealed record SessionResponse
{
    public required string Id { get; init; }
    public required string PersonaId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? LastActivityAt { get; init; }
    public string? ModelId { get; init; }
    public string? Endpoint { get; init; }
    public string? ServiceType { get; init; }
}
