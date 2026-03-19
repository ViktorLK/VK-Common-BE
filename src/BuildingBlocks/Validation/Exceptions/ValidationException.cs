using VK.Blocks.Core.Exceptions;
using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : BaseException
{
    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("ValidationErrors", "One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}
