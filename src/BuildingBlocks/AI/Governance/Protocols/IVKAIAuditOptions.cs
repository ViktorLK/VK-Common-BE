namespace VK.Blocks.AI;

/// <summary>
/// Defines audit configuration for AI features.
/// </summary>
public interface IVKAIAuditOptions
{
    /// <summary>
    /// Gets a value indicating whether to enable audit logging for this specific feature.
    /// If null, falls back to global AI options.
    /// </summary>
    bool? EnableAudit { get; init; }
}
