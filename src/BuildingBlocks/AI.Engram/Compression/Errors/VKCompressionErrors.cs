using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Compression stage.
/// </summary>
public static class VKCompressionErrors
{
    public static readonly VKError SessionNotFound = new("AI.Engram.Compression.SessionNotFound", "The specified chat session was not found.");
    public static readonly VKError CompressionFailed = new("AI.Engram.Compression.Failed", "The compression strategy failed to compress the content.");
    public static readonly VKError InvalidSession = new("AI.Engram.Compression.InvalidSession", "SessionId cannot be empty.");
    public static readonly VKError LockAcquisitionFailed = new("AI.Engram.Compression.LockAcquisitionFailed", "Compression lock is currently held by another task.");
    public static readonly VKError LlmSummaryError = new("AI.Engram.Compression.LlmSummaryError", "LLM summary compression encountered an error.");
    public static readonly VKError KeyValueExtractionError = new("AI.Engram.Compression.KeyValueExtractionError", "Key-value extraction compression encountered an error.");
}
