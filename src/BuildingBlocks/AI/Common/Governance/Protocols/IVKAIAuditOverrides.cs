namespace VK.Blocks.AI;

/// <summary>
/// Defines audit parameters that can be overridden at the request level.
/// </summary>
public interface IVKAIAuditOverrides
{
    /// <summary>
    /// Gets a value indicating whether to enable audit logging for this specific request.
    /// </summary>
    bool? EnableAudit { get; init; }
}
