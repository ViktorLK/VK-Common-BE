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
}
