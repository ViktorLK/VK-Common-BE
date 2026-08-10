using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Weaving feature.
/// </summary>
public static class VKWeavingErrors
{
    public static readonly VKError FormatterNotFound = new("AI.Weaving.FormatterNotFound", "No suitable formatter found for the given prompt segment.");
    public static readonly VKError NoTapestry = new("AI.Weaving.NoTapestry", "Tapestry was not produced by the weaving tasks.");
    public static readonly VKError EmptyActive = new("AI.Weaving.EmptyActive", "No active prompt fragments remaining after applying disabled tiers.");
}
