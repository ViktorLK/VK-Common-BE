using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.ChangeFeed.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Confliction.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Connection;
using VK.Blocks.Persistence.EFCore.Cosmos.Connection.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Failover.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Provisioning.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Query.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Repositories.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.ServerSide.Internal;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Common.DependencyInjection.Internal;

/// <summary>
/// Controls DI mapping for Cosmos DB building block.
/// </summary>
internal sealed partial class PersistenceEFCoreCosmosBlock
{
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreCosmosBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        var options = services.GetVKServiceInstance<VKPersistenceEFCoreCosmosOptions>()!;

        // 7. Core Services
        services.TryAddSingleton<IVKCosmosDbConnection, CosmosDbConnection>();

        services.AddDbContext<VKCosmosDbContext>((sp, dbBuilder) =>
        {
            var opt = sp.GetRequiredService<VKPersistenceEFCoreCosmosOptions>();
            dbBuilder.UseCosmos(opt.ConnectionString, opt.DatabaseName);
        });

        services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<VKCosmosDbContext>());
        services.TryAddScoped(typeof(IVKReadRepository<>), typeof(CosmosBaseRepository<>));
        services.TryAddScoped(typeof(IVKWriteRepository<>), typeof(CosmosBaseRepository<>));
        services.TryAddScoped(typeof(IVKBaseRepository<>), typeof(CosmosBaseRepository<>));
        services.TryAddScoped(typeof(IVKCosmosRepository<>), typeof(CosmosBaseRepository<>));
        services.TryAddSingleton(typeof(IVKCosmosQueryRepository<>), typeof(CosmosQueryRepository<>));

        services.TryAddSingleton<IVKCosmosServerSideManager, CosmosServerSideManager>();
        services.TryAddSingleton<IVKCosmosContainerProvisioner, CosmosContainerProvisioner>();
        services.TryAddSingleton<IVKCosmosFailoverManager, CosmosFailoverManager>();

        services.TryAddSingleton<CosmosIndexPolicyBuilder>();
        services.TryAddSingleton<CompositeIndexProvisioner>();
        services.TryAddSingleton<ChangeFeedObserverFactory>();
        services.TryAddSingleton<OptimisticConcurrencyHandler>();
        services.TryAddSingleton<CustomConflictResolver>();

        if (options.EnableSessionTokenPropagation)
        {
            services.TryAddSingleton<SessionTokenManager>();
            services.TryAddSingleton<IVKCosmosSessionTokenAccessor>(sp => sp.GetRequiredService<SessionTokenManager>());
        }

        services.TryAddSingleton<IVKCosmosTransactionalBatchFactory, CosmosTransactionalBatchFactory>();
    }
}
