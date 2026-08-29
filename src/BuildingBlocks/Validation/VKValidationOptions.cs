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

    /// <summary>
    /// Gets or sets a value indicating whether the validation pipeline should short-circuit upon first validator failure.
    /// Default is false (collect all errors from all validators).
    /// </summary>
    public bool ShortCircuitOnFirstFailure { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether applicable validators in the pipeline should be executed concurrently via Task.WhenAll.
    /// Default is false (sequential execution).
    /// </summary>
    public bool EnableParallelValidation { get; init; } = false;
}


