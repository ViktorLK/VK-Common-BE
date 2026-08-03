using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the Session thread feature in Psyche.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
public sealed partial record VKSessionOptions : IVKBlockOptions;
