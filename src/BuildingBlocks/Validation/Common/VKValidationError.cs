using System.Collections.Generic;

namespace VK.Blocks.Validation;

/// <summary>
/// Represents a single validation error with detailed metadata.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation.</param>
/// <param name="ErrorMessage">The validation error message.</param>
/// <param name="ErrorCode">The optional error code associated with the validation failure.</param>
/// <param name="Severity">The severity level of the error.</param>
/// <param name="AttemptedValue">The value that caused the validation failure, if safe to expose.</param>
/// <param name="Metadata">Additional custom metadata associated with the validation failure.</param>
public sealed record VKValidationError(
    string PropertyName,
    string ErrorMessage,
    string? ErrorCode = null,
    VKValidationSeverity Severity = VKValidationSeverity.Error,
    object? AttemptedValue = null,
    IReadOnlyDictionary<string, object>? Metadata = null);

