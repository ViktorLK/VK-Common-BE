namespace VK.Blocks.AI;

/// <summary>
/// Provides a base implementation for AI audit settings.
/// </summary>
public sealed record VKAIAuditSettings : IVKAIAuditOptions
{
    /// <inheritdoc />
    public bool? EnableAudit { get; init; }
}
