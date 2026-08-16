using System.Collections.Generic;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Contracts;

public sealed record CreatePersonaRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Personality { get; init; }
    public string? Scenario { get; init; }
    public string? FirstMessage { get; init; }
    public string? DialogueExamples { get; init; }
    public string? SystemPrompt { get; init; }
    public string? DirectiveId { get; init; }
    public Dictionary<string, string> Traits { get; init; } = [];
}
