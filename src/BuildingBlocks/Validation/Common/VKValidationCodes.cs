namespace VK.Blocks.Validation;

/// <summary>
/// Standardized machine-readable validation error codes.
/// </summary>
public static class VKValidationCodes
{
    /// <summary>Field is required or cannot be null/empty.</summary>
    public const string Required = "Validation.Required";

    /// <summary>String or collection length out of bounds.</summary>
    public const string Length = "Validation.Length";

    /// <summary>Numeric, date, or comparable value out of range.</summary>
    public const string Range = "Validation.Range";

    /// <summary>Format does not match required regular expression or pattern.</summary>
    public const string Pattern = "Validation.Pattern";

    /// <summary>Email address format is invalid.</summary>
    public const string Email = "Validation.Email";

    /// <summary>Enum value is out of defined range.</summary>
    public const string Enum = "Validation.Enum";

    /// <summary>Collection is empty or does not satisfy element count bounds.</summary>
    public const string Collection = "Validation.Collection";

    /// <summary>Unique constraint violation (e.g. duplicate username, email).</summary>
    public const string Unique = "Validation.Unique";

    /// <summary>Referenced target does not exist.</summary>
    public const string Exists = "Validation.Exists";

    /// <summary>Entity state transition or lifecycle rule violated.</summary>
    public const string State = "Validation.State";

    /// <summary>Operation not permitted under current validation rule.</summary>
    public const string Unauthorized = "Validation.Unauthorized";

    /// <summary>Specification criteria not satisfied.</summary>
    public const string Specification = "Validation.Specification";

    /// <summary>Custom business validation failed.</summary>
    public const string Custom = "Validation.Custom";
}
