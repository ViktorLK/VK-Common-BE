using System;
using System.Collections.Generic;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the centralized registry for managing and resolving named resilience pipelines.
/// Follows [AP.01], [AP.02].
/// </summary>
public interface IVKPolicyRegistry
{
    /// <summary>
    /// Gets all registered pipeline names.
    /// </summary>
    IReadOnlyCollection<string> RegisteredPipelineNames { get; }

    /// <summary>
    /// Gets a registered resilience pipeline by name. Throws <see cref="KeyNotFoundException"/> if not found.
    /// </summary>
    IVKResiliencePipeline GetPipeline(string pipelineName);

    /// <summary>
    /// Tries to get a registered resilience pipeline by name.
    /// </summary>
    bool TryGetPipeline(string pipelineName, out IVKResiliencePipeline? pipeline);

    /// <summary>
    /// Registers a resilience pipeline under the specified name.
    /// </summary>
    void RegisterPipeline(string pipelineName, IVKResiliencePipeline pipeline);

    /// <summary>
    /// Gets an existing pipeline or builds and registers a new one using the provided builder callback.
    /// </summary>
    IVKResiliencePipeline GetOrAddPipeline(
        string pipelineName,
        Func<IVKPolicyBuilder, IVKResiliencePipeline> configure);
}
