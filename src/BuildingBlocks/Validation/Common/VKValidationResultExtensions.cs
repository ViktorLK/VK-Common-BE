using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Extension methods for <see cref="VKValidationResult"/>.
/// </summary>
public static class VKValidationResultExtensions
{
    /// <summary>
    /// Throws a <see cref="VKValidationException"/> if the result is not valid.
    /// </summary>
    public static void ThrowIfInvalid(this VKValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new VKValidationException(result.Errors);
        }
    }

    /// <summary>
    /// Converts the <see cref="VKValidationResult"/> to a <see cref="VKValidationException"/>.
    /// </summary>
    public static VKValidationException ToException(this VKValidationResult result)
    {
        return new VKValidationException(result.Errors);
    }

    /// <summary>
    /// Converts the validation result into a standardized <see cref="VKResult"/>.
    /// </summary>
    public static VKResult ToResult(this VKValidationResult result)
    {
        if (result.IsValid)
        {
            return VKResult.Success();
        }

        var firstError = result.Errors.FirstOrDefault();
        var code = firstError?.ErrorCode ?? VKValidationCodes.Custom;
        var message = firstError?.ErrorMessage ?? "Validation failed.";

        var vkErrors = result.Errors.Select(e =>
            VKError.Validation(e.ErrorCode ?? VKValidationCodes.Custom, $"{e.PropertyName}: {e.ErrorMessage}"));

        return VKResult.Failure(vkErrors);
    }

    /// <summary>
    /// Converts the validation result into a strongly-typed <see cref="VKResult{T}"/>.
    /// </summary>
    public static VKResult<T> ToResult<T>(this VKValidationResult result, T value)
    {
        if (result.IsValid)
        {
            return VKResult.Success(value);
        }

        var vkErrors = result.Errors.Select(e =>
            VKError.Validation(e.ErrorCode ?? VKValidationCodes.Custom, $"{e.PropertyName}: {e.ErrorMessage}"));

        return VKResult.Failure<T>(vkErrors);
    }

}

