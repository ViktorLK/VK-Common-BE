using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Builder for configuring the Semantic Kernel building block.
/// </summary>
public interface IVKAISKBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    IConfiguration Configuration { get; }
}
