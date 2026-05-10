using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Resilience;

internal sealed class VKResilienceBuilder(IServiceCollection services) : IVKResilienceBuilder
{
    public IServiceCollection Services { get; } = services;
}
