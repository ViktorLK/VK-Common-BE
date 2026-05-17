namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all AI governance overrides for automated mapping.
/// </summary>
public interface IVKAIGovernanceOverrides :
    IVKAIResilienceOverrides,
    IVKAIAuditOverrides,
    IVKAIQuotaOverrides,
    IVKAISafetyOverrides
{
}
