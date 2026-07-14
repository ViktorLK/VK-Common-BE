using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Public interface for Cosmos DB query execution.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IVKCosmosQueryRepository<T> where T : class
{
    /// <summary>
    /// Executes a parameterized query using QueryDefinition and options.
    /// </summary>
    /// <param name="query">The parameterized query definition.</param>
    /// <param name="queryOptions">The query options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of items matching the query.</returns>
    Task<VKResult<IReadOnlyList<T>>> QueryAsync(
        QueryDefinition query,
        VKCosmosQueryOptions? queryOptions,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes a raw SQL query targeting a specific partition key.
    /// </summary>
    /// <param name="sql">The raw SQL query string.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of items matching the query.</returns>
    Task<VKResult<IReadOnlyList<T>>> QueryAsync(
        string sql,
        string? partitionKey,
        CancellationToken cancellationToken);
}
