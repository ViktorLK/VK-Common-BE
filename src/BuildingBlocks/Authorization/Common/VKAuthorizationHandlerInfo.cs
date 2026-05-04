namespace VK.Blocks.Authorization;

/// <summary>
/// Represents runtime information about a registered authorization handler or VKPermission evaluator.
/// </summary>
public sealed record VKAuthorizationHandlerInfo
{
    /// <summary>
    /// Gets the type name of the implementation class.
    /// </summary>
    public required string HandlerType { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a VKPermission evaluator (IVKPermissionEvaluator).
    /// </summary>
    public bool IsPermissionEvaluator { get; init; }

    /// <summary>
    /// Gets a descriptive name or category for the handler (if available).
    /// </summary>
    public string? DisplayName { get; init; }
}
