using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Error constants for the Compression stage.
/// </summary>
internal static class Errors
{
    public static readonly VKError SessionNotFound = new("AI.Engram.Compression.SessionNotFound", "The specified chat session was not found.");
    public static readonly VKError CompressionFailed = new("AI.Engram.Compression.Failed", "The compression strategy failed to compress the content.");
}
