using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public sealed partial record VKStorageOptions : IVKBlockOptions
{
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10MB default

    public string[] AllowedExtensions { get; init; } = [".jpg", ".jpeg", ".png", ".pdf"];

    /// <summary>
    /// Enables or disables soft-delete related operations.
    /// </summary>
    public bool EnableSoftDelete { get; init; } = false;

    /// <summary>
    /// Enables or disables versioning related operations.
    /// </summary>
    public bool EnableVersioning { get; init; } = false;
}
