namespace VK.Blocks.Validation;

/// <summary>
/// Represents a single validation error.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation.</param>
/// <param name="ErrorMessage">The validation error message.</param>
/// <param name="ErrorCode">The optional error code associated with the validation failure.</param>
public sealed record VKValidationError(string PropertyName, string ErrorMessage, string? ErrorCode = null);
