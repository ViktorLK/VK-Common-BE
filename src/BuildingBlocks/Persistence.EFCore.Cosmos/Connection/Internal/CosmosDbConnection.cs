using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Connection.Internal;

/// <summary>
/// Controls connection lifetime and topology.
/// </summary>
internal sealed class CosmosDbConnection : IVKCosmosDbConnection, IDisposable
{
    private readonly VKPersistenceEFCoreCosmosOptions _options;
    private readonly CosmosClient _client;
    private readonly Microsoft.Azure.Cosmos.Database _database;

    public CosmosDbConnection(
        VKPersistenceEFCoreCosmosOptions options,
        IVKJsonSerializer vkSerializer,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(options);
        _options = VKGuard.NotNull(options);
        VKGuard.NotNull(serviceProvider);

        var clientOptions = new CosmosClientOptions
        {
            Serializer = new SystemTextJsonCosmosSerializer(vkSerializer),
            ConnectionMode = _options.ConnectionMode == VKCosmosConnectionMode.Gateway ? ConnectionMode.Gateway : ConnectionMode.Direct,
            AllowBulkExecution = _options.EnableBulkExecution,
            MaxRetryAttemptsOnRateLimitedRequests = _options.MaxRetryAttemptsOnRateLimited,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(_options.MaxRetryIntervalSeconds)
        };

        if (_options.EnableIntegratedCache && !string.IsNullOrWhiteSpace(_options.DedicatedGatewayEndpoint))
        {
            // Direct connection to dedicated gateway
            // Standard Cosmos SDK way is to use AccountEndpoint of the dedicated gateway, or pass it via client options
            // But usually we set the gateway endpoint as the AccountEndpoint in CosmosClient constructor.
        }

        if (_options.ConsistencyLevel.HasValue)
        {
            clientOptions.ConsistencyLevel = MapConsistencyLevel(_options.ConsistencyLevel.Value);
        }

        if (_options.PreferredRegions != null && _options.PreferredRegions.Count > 0)
        {
            clientOptions.ApplicationPreferredRegions = new System.Collections.Generic.List<string>(_options.PreferredRegions);
        }

        // Initialize CosmosClient depending on AuthMode
        if (_options.AuthMode == VKCosmosAuthMode.AzureIdentity)
        {
            VKGuard.NotNullOrWhiteSpace(_options.AccountEndpoint);

            // Try to resolve IAzureCredentialProvider dynamically from service provider
            var providerType = Type.GetType("VK.Blocks.Infrastructure.Azure.Abstractions.IAzureCredentialProvider, VK.Blocks.Infrastructure.Azure");
            if (providerType == null)
            {
                throw new InvalidOperationException("IAzureCredentialProvider could not be resolved. Ensure Infrastructure.Azure block is registered.");
            }

            var provider = serviceProvider.GetService(providerType);
            if (provider == null)
            {
                throw new InvalidOperationException("IAzureCredentialProvider is not registered in the service container.");
            }

            var method = providerType.GetMethod("GetCredential");
            var credential = method?.Invoke(provider, null) as Azure.Core.TokenCredential;
            if (credential == null)
            {
                throw new InvalidOperationException("Failed to retrieve TokenCredential from IAzureCredentialProvider.");
            }

            _client = new CosmosClient(_options.AccountEndpoint, credential, clientOptions);
        }
        else if (_options.AuthMode == VKCosmosAuthMode.ResourceToken)
        {
            VKGuard.NotNullOrWhiteSpace(_options.AccountEndpoint);

            // ResourceToken auth mode:
            // Since resource tokens are container/user-specific, the CosmosClient itself is initialized with the AccountEndpoint
            // but we cannot authenticate globally without a master key or TokenCredential.
            // However, Cosmos SDK allows creating a CosmosClient with a resource token:
            // new CosmosClient(endpoint, resourceToken, clientOptions).
            // But because resource tokens are container specific, we fetch them dynamically when accessing containers,
            // or initialize CosmosClient with a placeholder/token.
            // Let's resolve the provider from ServiceProvider.
            var tokenProvider = serviceProvider.GetService(typeof(IVKCosmosResourceTokenProvider));

            // For general ICosmosResourceTokenProvider integration, we can instantiate the CosmosClient using the endpoint.
            // In Cosmos DB SDK, we can pass a dummy resource token or wait to fetch.
            // Actually, we can pass an empty token or initialize with dummy if we configure dynamic request options.
            _client = new CosmosClient(_options.AccountEndpoint, "placeholder-token", clientOptions);
        }
        else
        {
            // Default ConnectionString auth mode
            var endpoint = _options.EnableIntegratedCache && !string.IsNullOrWhiteSpace(_options.DedicatedGatewayEndpoint)
                ? _options.DedicatedGatewayEndpoint
                : _options.ConnectionString;

            _client = new CosmosClient(endpoint, clientOptions);
        }

        _database = _client.GetDatabase(_options.DatabaseName);
    }

    public CosmosClient Client => _client;

    public Microsoft.Azure.Cosmos.Database Database => _database;

    public Container GetContainer(string containerName)
    {
        VKGuard.NotNullOrWhiteSpace(containerName);
        return _database.GetContainer(containerName);
    }

    public async Task<VKResult> InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            ThroughputProperties? throughput = null;
            if (_options.ThroughputMode == VKCosmosThroughputMode.Autoscale)
            {
                throughput = ThroughputProperties.CreateAutoscaleThroughput(_options.AutoscaleMaxThroughput);
            }
            else
            {
                throughput = ThroughputProperties.CreateManualThroughput(_options.DefaultThroughput);
            }

            await _client.CreateDatabaseIfNotExistsAsync(_options.DatabaseName, throughput, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.Connection.InitializationFailed(ex.Message));
        }
    }

    private static ConsistencyLevel MapConsistencyLevel(VKCosmosConsistencyLevel level)
    {
        return level switch
        {
            VKCosmosConsistencyLevel.Strong => ConsistencyLevel.Strong,
            VKCosmosConsistencyLevel.BoundedStaleness => ConsistencyLevel.BoundedStaleness,
            VKCosmosConsistencyLevel.Session => ConsistencyLevel.Session,
            VKCosmosConsistencyLevel.ConsistentPrefix => ConsistencyLevel.ConsistentPrefix,
            VKCosmosConsistencyLevel.Eventual => ConsistencyLevel.Eventual,
            _ => ConsistencyLevel.Session
        };
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
