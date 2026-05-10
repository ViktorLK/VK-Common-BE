using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAIBuilder"/>.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The configuration.</param>
internal sealed class AIBlockBuilder(IServiceCollection services, IConfiguration? configuration) : IVKAIBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = VKGuard.NotNull(services);

    /// <inheritdoc />
    public IConfiguration? Configuration { get; } = configuration;
}
