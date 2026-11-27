namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Constants for AI.Synapse diagnostics, OpenTelemetry semantic tokens, and metric meters.
/// </summary>
public static class VKAISynapseDiagnosticsConstants
{
    public const string TotalRequests = "vk.ai.synapse.requests.count";
    public const string RequestDuration = "vk.ai.synapse.request.duration";
    public const string TokensConsumed = "vk.ai.synapse.tokens.consumed";
    public const string CostCalculated = "vk.ai.synapse.cost.calculated";
    public const string ProviderFailures = "vk.ai.synapse.provider.failures.count";
}
