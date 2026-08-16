using System.Collections.Generic;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Contracts;

public sealed record PersonaResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? DirectiveId { get; init; }
    public Dictionary<string, string> Traits { get; init; } = [];
}
