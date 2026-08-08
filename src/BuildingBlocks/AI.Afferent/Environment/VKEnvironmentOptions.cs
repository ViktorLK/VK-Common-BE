using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Configuration options for the Environment feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>

public sealed partial record VKEnvironmentOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Environment is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to capture OCR text from the screen.
    /// Defaults to true.
    /// </summary>
    public bool EnableOcr { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to capture screen metadata.
    /// Defaults to true.
    /// </summary>
    public bool CaptureWindowMetadata { get; init; } = true;
}
