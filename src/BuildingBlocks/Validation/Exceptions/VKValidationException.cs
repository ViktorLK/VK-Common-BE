using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public sealed class VKValidationException : VKBaseException
{
    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<VKValidationError> Errors { get; }

    public VKValidationException(IEnumerable<VKValidationError> errors)
        : base("ValidationErrors", "One or more validation errors occurred.")
    {
        Errors = [.. errors];
    }
}
