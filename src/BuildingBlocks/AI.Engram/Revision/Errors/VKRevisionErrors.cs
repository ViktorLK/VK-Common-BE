using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Revision subsystem.
/// Follows CS.01.
/// </summary>
public static class VKRevisionErrors
{
    public static readonly VKError InvalidVersion = new("AI.Engram.Revision.InvalidVersion", "Target version must be greater than or equal to 1.");
    public static readonly VKError NotFound = new("AI.Engram.Revision.NotFound", "Memory entry not found for revision.");
    public static readonly VKError FutureVersion = new("AI.Engram.Revision.FutureVersion", "Cannot rollback to a version which is higher than current version.");
    public static readonly VKError VersionNotFound = new("AI.Engram.Revision.VersionNotFound", "History for the specified version is not available in metadata.");
    public static readonly VKError AnalysisError = new("AI.Engram.Revision.AnalysisError", "LLM revision change analysis failed.");
}
