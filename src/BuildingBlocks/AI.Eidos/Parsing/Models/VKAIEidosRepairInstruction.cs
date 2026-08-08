using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public sealed record VKAIEidosRepairInstruction
{
    public required string CorrectivePrompt { get; init; }
    public int AttemptCount { get; init; } = 1;
}
