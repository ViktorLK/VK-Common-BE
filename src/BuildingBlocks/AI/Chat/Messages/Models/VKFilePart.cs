using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a generic file part of a message (e.g., PDF, Document, or generic binary data).
/// </summary>
public sealed record VKFilePart : IVKChatMessagePart
{
    /// <inheritdoc />
    public string PartType => "file";

    /// <summary>
    /// Gets the source URI or encoded data of the file.
    /// </summary>
    public required string FileSource { get; init; }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the MIME type of the file.
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets additional metadata for the file.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}
