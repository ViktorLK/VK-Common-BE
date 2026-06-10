namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents relative layout positioning using a logical system prompt anchor.
/// </summary>
public sealed record VKRelativePromptPosition(VKPromptRelativeAnchor Relative, int Priority = 0) : IVKPromptPosition;
