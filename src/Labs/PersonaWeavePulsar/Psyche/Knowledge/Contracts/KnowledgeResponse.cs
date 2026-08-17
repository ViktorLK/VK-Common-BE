using System.Collections.Generic;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Contracts;

public sealed record KnowledgeResponse
{
    public required string Id { get; init; }
    public required string BookId { get; init; }
    public string? Name { get; init; }
    public string? Content { get; init; }
    public bool IsEnabled { get; init; }
    public int Priority { get; init; }
    public string? Tag { get; init; }
    public List<string> Keys { get; init; } = [];
}
