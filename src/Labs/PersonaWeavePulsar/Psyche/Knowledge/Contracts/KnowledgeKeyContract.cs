namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Contracts;

public sealed record KnowledgeKeyContract
{
    public required string Text { get; init; }
    public bool? IsRegex { get; init; }
    public bool? IsFilter { get; init; }
    public int? Logic { get; init; }
}
