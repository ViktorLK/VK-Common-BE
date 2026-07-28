using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Standard error constants for the Structured Memory feature.
/// Follows CS.01.
/// </summary>
public static class VKStructuredErrors
{
    /// <summary>
    /// Error returned when the requested fact key is not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Engram.Structured.NotFound", "The requested fact key was not found.");

    /// <summary>
    /// Error returned when stored fact value type does not match expected type.
    /// </summary>
    public static readonly VKError TypeMismatch = new("AI.Engram.Structured.TypeMismatch", "Stored fact value type does not match requested type.");

    /// <summary>
    /// Error returned when schema/type validation fails during StoreFactAsync.
    /// </summary>
    public static readonly VKError SchemaValidationFailed = new("AI.Engram.Structured.SchemaValidationFailed", "Fact value failed schema or type validation.");

    /// <summary>
    /// Error returned when accessing a fact across unauthorized scope/tenant boundaries.
    /// </summary>
    public static readonly VKError ScopeViolation = new("AI.Engram.Structured.ScopeViolation", "Tenant or scope boundary violation detected.");

    /// <summary>
    /// Error returned when the maximum fact capacity per tenant is reached.
    /// </summary>
    public static readonly VKError CapacityExceeded = new("AI.Engram.Structured.CapacityExceeded", "Maximum structured fact capacity for tenant has been reached.");
}
