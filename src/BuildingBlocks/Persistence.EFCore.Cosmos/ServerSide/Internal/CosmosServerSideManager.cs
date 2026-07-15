using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.Common.Diagnostics.Internal;
using VK.Blocks.Persistence.EFCore.Cosmos.Connection;

namespace VK.Blocks.Persistence.EFCore.Cosmos.ServerSide.Internal;

/// <summary>
/// Implementation of server-side script management using Cosmos SDK Scripts container property.
/// </summary>
internal sealed class CosmosServerSideManager : IVKCosmosServerSideManager
{
    private readonly IVKCosmosDbConnection _dbConnection;
    private readonly ILogger<CosmosServerSideManager> _logger;

    public CosmosServerSideManager(IVKCosmosDbConnection dbConnection, ILogger<CosmosServerSideManager> logger)
    {
        _dbConnection = VKGuard.NotNull(dbConnection);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> RegisterStoredProcedureAsync(
        string containerName,
        string id,
        string body,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(containerName);
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNullOrWhiteSpace(body);

        try
        {
            var container = _dbConnection.GetContainer(containerName);
            var properties = new StoredProcedureProperties(id, body);

            try
            {
                await container.Scripts.ReplaceStoredProcedureAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                await container.Scripts.CreateStoredProcedureAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.ServerSide.RegistrationFailed("StoredProcedure", id, ex.Message));
        }
    }

    public async Task<VKResult<T>> ExecuteStoredProcedureAsync<T>(
        string containerName,
        string id,
        string partitionKey,
        object[] parameters,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(containerName);
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNullOrWhiteSpace(partitionKey);
        VKGuard.NotNull(parameters);

        try
        {
            var container = _dbConnection.GetContainer(containerName);
            var response = await container.Scripts.ExecuteStoredProcedureAsync<T>(
                id,
                new PartitionKey(partitionKey),
                parameters,
                cancellationToken: ct).ConfigureAwait(false);

            CosmosLog.LogStoredProcedureExecuted(_logger, id, containerName, response.RequestCharge);
            return VKResult.Success(response.Resource);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<T>(Errors.ServerSide.ExecutionFailed(id, ex.Message));
        }
    }

    public async Task<VKResult> RegisterTriggerAsync(
        string containerName,
        VKCosmosTriggerDefinition definition,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(containerName);
        VKGuard.NotNull(definition);

        try
        {
            var container = _dbConnection.GetContainer(containerName);
            var properties = new TriggerProperties
            {
                Id = definition.Id,
                Body = definition.Body,
                TriggerType = definition.TriggerType,
                TriggerOperation = definition.TriggerOperation
            };

            try
            {
                await container.Scripts.ReplaceTriggerAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                await container.Scripts.CreateTriggerAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.ServerSide.RegistrationFailed("Trigger", definition.Id, ex.Message));
        }
    }

    public async Task<VKResult> RegisterUserDefinedFunctionAsync(
        string containerName,
        string id,
        string body,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(containerName);
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNullOrWhiteSpace(body);

        try
        {
            var container = _dbConnection.GetContainer(containerName);
            var properties = new UserDefinedFunctionProperties
            {
                Id = id,
                Body = body
            };

            try
            {
                await container.Scripts.ReplaceUserDefinedFunctionAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                await container.Scripts.CreateUserDefinedFunctionAsync(properties, cancellationToken: ct).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.ServerSide.RegistrationFailed("UserDefinedFunction", id, ex.Message));
        }
    }
}
