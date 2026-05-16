namespace VK.Blocks.AI;

/// <summary>
/// Defines injection guard parameters that can be overridden at the request level.
/// </summary>
public interface IVKInjectionOverrides :
    IVKAIProviderOverrides,
    IVKAIResilienceOverrides,
    IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the threshold for blocking a request based on confidence score.
    /// </summary>
    float? BlockThreshold { get; init; }
}
