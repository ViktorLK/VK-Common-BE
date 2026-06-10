namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents absolute numerical layout positioning specifying a target role and numerical depth.
/// </summary>
public sealed record VKAbsolutePromptPosition(VKChatRole Role, int Depth) : IVKPromptPosition;
