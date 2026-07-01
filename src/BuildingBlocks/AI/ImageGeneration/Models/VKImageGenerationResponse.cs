using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the structured response from an image generation engine.
/// </summary>
public sealed record VKImageGenerationResponse
{
    /// <summary>
    /// Gets the image data as base64 string or the URL of the generated image.
    /// </summary>
    public required string ImageSource { get; init; }

    /// <summary>
    /// Gets the MIME type of the generated image (e.g., "image/png").
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets the identifier of the model that generated the image.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets additional metadata for the generation.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}
