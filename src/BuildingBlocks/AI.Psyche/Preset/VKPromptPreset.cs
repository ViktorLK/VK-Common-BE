using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain model representing a Prompt Preset and Sampling Configuration template in AI.Psyche.
/// </summary>
public sealed record VKPromptPreset
{
    public required string Id { get; init; }
    public VKTenantId? TenantId { get; init; }
    public required string Name { get; init; }
    public string? TargetConnectionId { get; init; }
    public string? MainPrompt { get; init; }
    public string? PostInstructions { get; init; }
    public string? AuthorsNote { get; init; }
    public int AuthorsNoteDepth { get; init; } = 1;
    public VKGenerationOptions Options { get; init; } = new();
}
