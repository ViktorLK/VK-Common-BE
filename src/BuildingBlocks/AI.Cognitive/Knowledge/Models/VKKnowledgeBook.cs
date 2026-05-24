
namespace VK.Blocks.AI.Cognitive;

public sealed record VKKnowledgeBook
{
    public required string Id { get; init; }
    public required string TenantId { get; init; }
    public required string Name { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsGlobal { get; init; } = false;
    public string? Description { get; init; }

}
