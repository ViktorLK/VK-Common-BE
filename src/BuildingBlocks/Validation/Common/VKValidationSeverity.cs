namespace VK.Blocks.Validation;

/// <summary>
/// Specifies the severity level of a validation error.
/// </summary>
public enum VKValidationSeverity : byte
{
    /// <summary>
    /// Represents a validation error that prevents the operation from continuing.
    /// </summary>
    Error = 0,

    /// <summary>
    /// Represents a warning that does not block execution but should be highlighted.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Represents informational feedback.
    /// </summary>
    Info = 2
}

