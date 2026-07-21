namespace VK.Blocks.Authorization;

/// <summary>
/// Defines how multiple permissions should be evaluated.
/// </summary>
public enum VKPermissionEvaluationMode
{
    /// <summary>
    /// Requirement is met only if ALL specified permissions are granted. (AND logic)
    /// </summary>
    All,

    /// <summary>
    /// Requirement is met if ANY of the specified permissions are granted. (OR logic)
    /// </summary>
    Any
}
