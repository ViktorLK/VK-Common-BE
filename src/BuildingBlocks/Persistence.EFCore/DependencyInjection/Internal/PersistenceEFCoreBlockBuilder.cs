using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPersistenceEFCoreBuilder"/>.
/// </summary>
internal sealed class PersistenceEFCoreBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKPersistenceEFCoreBlock>(services, configuration), IVKPersistenceEFCoreBuilder;
