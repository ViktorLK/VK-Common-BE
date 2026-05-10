using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for building a resilience pipeline.
/// </summary>
public interface IVKResiliencePipelineBuilder
{
    /// <summary>
    /// Adds a retry strategy to the pipeline.
    /// </summary>
    IVKResiliencePipelineBuilder AddRetry(VKRetryOptions options);

    /// <summary>
    /// Adds a circuit breaker strategy to the pipeline.
    /// </summary>
    IVKResiliencePipelineBuilder AddCircuitBreaker(VKCircuitBreakerOptions options);

    /// <summary>
    /// Adds a timeout strategy to the pipeline.
    /// </summary>
    IVKResiliencePipelineBuilder AddTimeout(VKTimeoutOptions options);

    /// <summary>
    /// Adds a bulkhead strategy to the pipeline.
    /// </summary>
    IVKResiliencePipelineBuilder AddBulkhead(VKBulkheadOptions options);

    /// <summary>
    /// Builds the resilience pipeline.
    /// </summary>
    IVKResiliencePipeline Build();
}
