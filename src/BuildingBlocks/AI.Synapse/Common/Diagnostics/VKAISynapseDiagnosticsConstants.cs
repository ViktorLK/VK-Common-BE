namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Constants for AI.Synapse diagnostics, OpenTelemetry semantic tokens, and metric meters.
/// </summary>
public static class VKAISynapseDiagnosticsConstants
{
    public const string TotalRequests = "vk.ai.gateway.requests.count";
    public const string RequestDuration = "vk.ai.gateway.request.duration";
    public const string TokensConsumed = "vk.ai.gateway.tokens.consumed";
    public const string CostCalculated = "vk.ai.gateway.cost.calculated";
    public const string ProviderFailures = "vk.ai.gateway.provider.failures.count";
    public const string CircuitBreakerTrips = "vk.ai.gateway.circuit_breaker.trips.count";
}
