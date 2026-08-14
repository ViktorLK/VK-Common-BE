using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain resolver for calculating 3-Tier Cascade Generation Hyperparameters.
/// Fallback order: Session Override > Preset Override > Tenant Default.
/// </summary>
public static class VKGenerationOptionsResolver
{
    public static VKGenerationOptions Resolve(
        VKGenerationOptions? sessionOverride,
        VKGenerationOptions? presetOverride,
        VKGenerationOptions? tenantDefault)
    {
        return new VKGenerationOptions
        {
            Temperature = sessionOverride?.Temperature ?? presetOverride?.Temperature ?? tenantDefault?.Temperature ?? 0.7f,
            TopP = sessionOverride?.TopP ?? presetOverride?.TopP ?? tenantDefault?.TopP ?? 1.0f,
            TopK = sessionOverride?.TopK ?? presetOverride?.TopK ?? tenantDefault?.TopK,
            MaxTokens = sessionOverride?.MaxTokens ?? presetOverride?.MaxTokens ?? tenantDefault?.MaxTokens ?? 2048,
            FrequencyPenalty = sessionOverride?.FrequencyPenalty ?? presetOverride?.FrequencyPenalty ?? tenantDefault?.FrequencyPenalty ?? 0.0f,
            PresencePenalty = sessionOverride?.PresencePenalty ?? presetOverride?.PresencePenalty ?? tenantDefault?.PresencePenalty ?? 0.0f
        };
    }
}
