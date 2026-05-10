using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the builder for the Resilience building block.
/// </summary>
public interface IVKResilienceBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}
