namespace VK.Blocks.Authorization.Diagnostics.Models;

/// <summary>
/// Represents runtime information about a registered authorization handler or permission evaluator.
/// </summary>
public sealed record AuthorizationHandlerInfo
{
    /// <summary>
    /// Gets the type name of the implementation class.
    /// </summary>
    public required string HandlerType { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a permission evaluator (IPermissionEvaluator).
    /// </summary>
    public bool IsPermissionEvaluator { get; init; }

    /// <summary>
    /// Gets a descriptive name or category for the handler (if available).
    /// </summary>
    public string? DisplayName { get; init; }
}
