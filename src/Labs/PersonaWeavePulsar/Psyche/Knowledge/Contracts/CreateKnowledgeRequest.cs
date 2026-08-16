using System.Collections.Generic;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Contracts;

public sealed record CreateKnowledgeRequest
{
    public string? BookId { get; init; }
    public string? Memo { get; init; }
    public required string Content { get; init; }
    public int? Strategy { get; init; }
    public int? Priority { get; init; }
    public string? Position { get; init; }
    public int? Depth { get; init; }
    public string? Role { get; init; }
    public int? Probability { get; init; }
    public IEnumerable<string>? InclusionGroups { get; init; }
    public int? GroupWeight { get; init; }
    public int? TimedSticky { get; init; }
    public int? TimedCooldown { get; init; }
    public int? TimedDelay { get; init; }
    public bool? IsEnabled { get; init; }
    public string? Tag { get; init; }
    public List<KnowledgeKeyContract>? Keys { get; init; }
}
