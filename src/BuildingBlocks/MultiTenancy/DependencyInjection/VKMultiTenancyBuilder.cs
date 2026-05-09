using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Default implementation of <see cref="IVKMultiTenancyBuilder"/>.
/// </summary>
public sealed class VKMultiTenancyBuilder(IServiceCollection services, IConfiguration configuration) : IVKMultiTenancyBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = VKGuard.NotNull(services);

    /// <inheritdoc />
    public IConfiguration Configuration { get; } = VKGuard.NotNull(configuration);
}
