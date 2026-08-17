namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Contracts;

public sealed record UpdatePatternRequest
{
    public string Name { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int? TargetRole { get; init; }
    public int? AbsoluteDepth { get; init; }
    public int? RelativeAnchor { get; init; }
    public int? Priority { get; init; }
}
