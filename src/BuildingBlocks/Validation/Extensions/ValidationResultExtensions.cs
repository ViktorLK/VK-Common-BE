using VK.Blocks.Validation.Abstractions.Contracts;
using VK.Blocks.Validation.Exceptions;

namespace VK.Blocks.Validation.Extensions;

/// <summary>
/// Extension methods for <see cref="ValidationResult"/>.
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Throws a <see cref="ValidationException"/> if the result is not valid.
    /// </summary>
    public static void ThrowIfInvalid(this ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    /// <summary>
    /// Converts the <see cref="ValidationResult"/> to a <see cref="ValidationException"/>.
    /// </summary>
    public static ValidationException ToException(this ValidationResult result)
    {
        return new ValidationException(result.Errors);
    }
}
