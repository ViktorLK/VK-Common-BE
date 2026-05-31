namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents relative layout positioning using a logical system prompt anchor.
/// Complies with AP.01 and AP.03.
/// </summary>
public sealed record VKKnowledgeRelativePosition(VKKnowledgeRelative Relative, int Priority = 0) : IVKKnowledgePosition;
