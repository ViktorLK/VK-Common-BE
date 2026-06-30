namespace VK.Blocks.AI;

/// <summary>
/// Defines governance parameters for AI features, aggregating resilience, audit, quota, and safety configuration.
/// </summary>
public interface IVKAIGovernanceOptions :
    IVKAIResilienceOptions,
    IVKAIAuditOptions,
    IVKAIQuotaOptions,
    IVKAISafetyOptions
{
}
