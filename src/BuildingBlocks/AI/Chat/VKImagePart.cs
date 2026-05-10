namespace VK.Blocks.AI;

/// <summary>
/// Represents an image part of a message.
/// </summary>
public sealed record VKImagePart : IVKChatMessagePart
{
    /// <inheritdoc />
    public string PartType => "image";

    /// <summary>
    /// Gets the URI or Base64 data of the image.
    /// </summary>
    public required string ImageSource { get; init; }

    /// <summary>
    /// Gets the MIME type of the image (e.g., "image/jpeg").
    /// </summary>
    public string? MimeType { get; init; }
}
