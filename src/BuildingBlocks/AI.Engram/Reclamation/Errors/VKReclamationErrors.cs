using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Reclamation (Decay & Pruning) subsystem.
/// Follows CS.01.
/// </summary>
public static class VKReclamationErrors
{
    public static readonly VKError CycleError = new("AI.Engram.Reclamation.CycleError", "Memory reclamation cycle encountered an unhandled error.");
}
