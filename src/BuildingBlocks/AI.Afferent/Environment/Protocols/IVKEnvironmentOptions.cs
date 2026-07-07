using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the public contract interface for Environment configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKEnvironmentOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether to capture OCR text from the screen.
    /// </summary>
    bool EnableOcr { get; }

    /// <summary>
    /// Gets a value indicating whether to capture screen metadata (e.g., active window title).
    /// </summary>
    bool CaptureWindowMetadata { get; }
}
