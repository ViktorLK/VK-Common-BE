using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Result of contract negotiation before dispatching request.
/// </summary>
public sealed record VKAIEidosNegotiationResult
{
    public required VKAIEidosExpressionMode SelectedMode { get; init; }
    public required VKAIEidosResponseContract Contract { get; init; }
    public string? SystemPromptInstruction { get; init; }
}
