using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Configuration options for the validation module.
/// </summary>
public sealed record VKValidationOptions : IVKBlockOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public static string SectionName => VKBlocksConstants.VKBlocksConfigPrefix + "Validation";

    /// <summary>
    /// Gets or sets a value indicating whether the block is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether DataAnnotations validation is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableDataAnnotations { get; init; } = true;
}
