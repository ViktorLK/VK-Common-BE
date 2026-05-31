namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents absolute numerical layout positioning specifying a target role and numerical depth.
/// Complies with AP.01 and AP.03.
/// </summary>
public sealed record VKKnowledgeAbsolutePosition(VKChatRole Role, int Depth) : IVKKnowledgePosition;
