using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Persistence.Cosmos.Repositories.Internal;

/// <summary>
/// Cosmos DB repository implementation that leverages EF Core Cosmos provider and native SDK.
/// </summary>
internal sealed class CosmosBaseRepository<T> : VKEFCoreReadRepository<T>, IVKCosmosRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly IVKGuidGenerator _guidGenerator;

    public CosmosBaseRepository(
        DbContext context,
        ILogger<CosmosBaseRepository<T>> logger,
        IVKCursorSerializer cursorSerializer,
        IVKGuidGenerator guidGenerator)
        : base(context, logger, cursorSerializer)
    {
        _context = VKGuard.NotNull(context); // [AP.01]
        _guidGenerator = VKGuard.NotNull(guidGenerator); // [AP.01]
    }

    /// <inheritdoc />
    public Container GetNativeContainer()
    {
        var cosmosClient = _context.Database.GetCosmosClient();
        var databaseId = _context.Database.GetCosmosDatabaseId();
        var entityType = _context.Model.FindEntityType(typeof(T));
        var containerId = entityType?.GetContainer() ?? typeof(T).Name;
        return cosmosClient.GetDatabase(databaseId).GetContainer(containerId);
    }

    /// <inheritdoc />
    public CosmosClient GetNativeClient()
    {
        return _context.Database.GetCosmosClient();
    }

    private string GetId(T entity)
    {
        var prop = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("id");
        return prop?.GetValue(entity)?.ToString() ?? _guidGenerator.Create().ToString();
    }

    // --- IVKWriteRepository ---

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entity); // [AP.01]
        await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false); // [CS.03]
        return entity;
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entities); // [AP.01]
        
        // Performance optimization: Bulk insert using Native SDK instead of EF Core SaveChanges
        var container = GetNativeContainer();
        var tasks = new List<Task>(entities.Count);
        foreach (var entity in entities)
        {
            var partitionKey = PartitionKeyRouter.ComputePartitionKey(entity);
            tasks.Add(container.CreateItemAsync(entity, partitionKey, cancellationToken: cancellationToken));
        }
        await Task.WhenAll(tasks).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public ValueTask UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entity); // [AP.01]
        DbSet.Update(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateRangeAsync(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entities); // [AP.01]
        DbSet.UpdateRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entity); // [AP.01]
        
        // Native atomic Upsert using Cosmos SDK Container
        var container = GetNativeContainer();
        var partitionKey = PartitionKeyRouter.ComputePartitionKey(entity);
        var response = await container.UpsertItemAsync(entity, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
        return response.Resource;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entity); // [AP.01]
        DbSet.Remove(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteRangeAsync(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entities); // [AP.01]
        DbSet.RemoveRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask HardDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entity); // [AP.01]
        
        // Physical delete bypassing EF Core soft-delete interceptors
        var container = GetNativeContainer();
        var id = GetId(entity);
        var partitionKey = PartitionKeyRouter.ComputePartitionKey(entity);
        await container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async ValueTask HardDeleteRangeAsync(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entities); // [AP.01]
        
        // Physical bulk delete using Native SDK
        var container = GetNativeContainer();
        var tasks = new List<Task>(entities.Count);
        foreach (var entity in entities)
        {
            var id = GetId(entity);
            var partitionKey = PartitionKeyRouter.ComputePartitionKey(entity);
            tasks.Add(container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken));
        }
        await Task.WhenAll(tasks).ConfigureAwait(false); // [CS.03]
    }
}
