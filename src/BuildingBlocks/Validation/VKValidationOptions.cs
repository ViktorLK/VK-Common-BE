using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Configuration options for the validation module.
/// </summary>
public sealed partial record VKValidationOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether DataAnnotations validation is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableDataAnnotations { get; init; } = true;
}
