using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Sqlite.DependencyInjection.Internal;

internal sealed class PersistenceSqliteBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKPersistenceSqliteBlock>(services, configuration), IVKPersistenceSqliteBuilder;
