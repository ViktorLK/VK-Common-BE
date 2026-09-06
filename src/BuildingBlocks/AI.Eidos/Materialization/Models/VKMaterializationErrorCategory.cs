namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Categories of contract validation failures for structured diagnostics and precision self-healing.
/// </summary>
public enum VKMaterializationErrorCategory : byte
{
    /// <summary>
    /// Unspecified or general validation error.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Malformed JSON, unclosed quotes/braces, or unexpected truncation.
    /// </summary>
    Syntax = 1,

    /// <summary>
    /// A required property defined in the schema is missing from the JSON payload.
    /// </summary>
    MissingRequired = 2,

    /// <summary>
    /// Property value kind does not match the expected schema data type (e.g. string vs number).
    /// </summary>
    TypeMismatch = 3,

    /// <summary>
    /// Value violates specific constraints (e.g. enum undefined value, regex mismatch, range limits).
    /// </summary>
    ConstraintViolation = 4,

    /// <summary>
    /// Payload contains extraneous fields prohibited by strict schema rules (additionalProperties: false).
    /// </summary>
    ExtraneousFields = 5
}
