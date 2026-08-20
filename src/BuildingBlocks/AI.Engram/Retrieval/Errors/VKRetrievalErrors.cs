using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Retrieval / Predictive Prefetch subsystem.
/// Follows CS.01.
/// </summary>
public static class VKRetrievalErrors
{
    public static readonly VKError PrefetchError = new("AI.Engram.Retrieval.PrefetchError", "Predictive memory prefetch encountered an error.");
}
