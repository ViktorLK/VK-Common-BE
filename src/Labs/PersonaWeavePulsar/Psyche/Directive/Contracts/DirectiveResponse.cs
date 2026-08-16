namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Contracts;

public sealed record DirectiveResponse
{
    public required string Id { get; init; }
    public string? Overview { get; init; }
    public string? BehaviorRules { get; init; }
    public string? SafetyRules { get; init; }
    public string? OutputConstraints { get; init; }
}
