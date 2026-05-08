using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPersistenceBuilder"/>.
/// </summary>
internal sealed class PersistenceBlockBuilder(IServiceCollection services, IConfiguration configuration) : IVKPersistenceBuilder
{
    public IServiceCollection Services { get; } = VKGuard.NotNull(services);
    public IConfiguration Configuration { get; } = VKGuard.NotNull(configuration);
}
