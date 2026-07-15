using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Public interface for managing and executing Cosmos DB server-side scripts (stored procedures, triggers, and UDFs).
/// </summary>
public interface IVKCosmosServerSideManager
{
    /// <summary>
    /// Registers or updates a stored procedure in a container.
    /// </summary>
    Task<VKResult> RegisterStoredProcedureAsync(
        string containerName,
        string id,
        string body,
        CancellationToken ct);

    /// <summary>
    /// Executes a stored procedure against a specific partition key.
    /// </summary>
    Task<VKResult<T>> ExecuteStoredProcedureAsync<T>(
        string containerName,
        string id,
        string partitionKey,
        object[] parameters,
        CancellationToken ct);

    /// <summary>
    /// Registers or updates a trigger in a container.
    /// </summary>
    Task<VKResult> RegisterTriggerAsync(
        string containerName,
        VKCosmosTriggerDefinition definition,
        CancellationToken ct);

    /// <summary>
    /// Registers or updates a User-Defined Function (UDF) in a container.
    /// </summary>
    Task<VKResult> RegisterUserDefinedFunctionAsync(
        string containerName,
        string id,
        string body,
        CancellationToken ct);
}
