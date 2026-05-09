using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Validation.DependencyInjection.Internal;

/// <summary>
/// realization of <see cref="IVKValidationBuilder"/> that configures the Validation building block.
/// </summary>
internal sealed class ValidationBuilder(IServiceCollection services, IConfiguration configuration) : IVKValidationBuilder
{
    public IServiceCollection Services { get; } = VKGuard.NotNull(services);
    public IConfiguration Configuration { get; } = VKGuard.NotNull(configuration);
}
