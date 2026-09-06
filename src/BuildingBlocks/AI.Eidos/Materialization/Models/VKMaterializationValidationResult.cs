using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Result container for contract validation, containing validation status and taxonomy-categorized errors.
/// Complies with [AP.01].
/// </summary>
public sealed record VKMaterializationValidationResult // [AP.01]
{
    public bool IsValid { get; init; } = true;
    public IReadOnlyList<string> MissingProperties { get; init; } = [];
    public IReadOnlyList<string> ErrorMessages { get; init; } = [];
    public IReadOnlyList<VKMaterializationValidationError> Errors { get; init; } = [];

    /// <summary>
    /// Static factory for successful validation.
    /// </summary>
    public static VKMaterializationValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Static factory for validation failures with structured taxonomy errors.
    /// </summary>
    public static VKMaterializationValidationResult Failure(IReadOnlyList<VKMaterializationValidationError> errors)
    {
        VKGuard.NotNull(errors);

        var missing = errors
            .Where(e => e.Category == VKMaterializationErrorCategory.MissingRequired)
            .Select(e => e.PropertyPath)
            .ToList();

        var messages = errors
            .Select(e => e.Message)
            .ToList();

        return new VKMaterializationValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            MissingProperties = missing,
            ErrorMessages = messages
        };
    }
}
