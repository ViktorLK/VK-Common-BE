namespace VK.Blocks.Validation.Options;

/// <summary>
/// Configuration options for the validation module.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether FluentValidation is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableFluentValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether DataAnnotations validation is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableDataAnnotations { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically validate requests in the middleware.
    /// Default is true.
    /// </summary>
    public bool EnableAutomaticValidation { get; set; } = true;
}
