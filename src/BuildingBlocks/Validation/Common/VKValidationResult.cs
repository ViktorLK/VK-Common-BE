using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed record VKValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<VKValidationError> Errors { get; init; } = Array.Empty<VKValidationError>();

    private static readonly VKValidationResult CachedSuccess = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static VKValidationResult Success() => CachedSuccess;

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static VKValidationResult Failure(IEnumerable<VKValidationError> errors)
    {
        VKGuard.NotNull(errors);
        return new VKValidationResult
        {
            Errors = errors as IReadOnlyList<VKValidationError> ?? errors.ToList()
        };
    }

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static VKValidationResult Failure(string propertyName, string message, string? errorCode = null)
        => new() { Errors = [new VKValidationError(propertyName, message, errorCode)] };
}
