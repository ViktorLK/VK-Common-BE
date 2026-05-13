using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAISKBuilder"/>.
/// </summary>
internal sealed class AISKBlockBuilder(IServiceCollection services, IConfiguration? configuration) : IVKAISKBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IConfiguration Configuration { get; } = configuration!;
}
