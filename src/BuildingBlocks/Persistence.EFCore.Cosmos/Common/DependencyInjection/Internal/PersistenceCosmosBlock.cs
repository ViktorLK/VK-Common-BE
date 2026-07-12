using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Persistence.Cosmos;

namespace VK.Blocks.Persistence.Cosmos.Common.DependencyInjection.Internal;

/// <summary>
/// Partial implementation for Persistence.Cosmos Defaults feature hooks.
/// Matches the inferred name 'PersistenceCosmosDefaults' from VKPersistenceCosmosOptions.
/// </summary>
internal sealed partial class PersistenceCosmosBlock
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPersistenceCosmosOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add global validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKPersistenceCosmosOptions options, List<string> failures)
    {
        if (options.AuthMode == VKCosmosAuthMode.ConnectionString && string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add("ConnectionString is required when Cosmos Persistence is enabled with ConnectionString authentication.");
        }

        if ((options.AuthMode == VKCosmosAuthMode.AzureIdentity || options.AuthMode == VKCosmosAuthMode.ResourceToken) && string.IsNullOrWhiteSpace(options.AccountEndpoint))
        {
            failures.Add("AccountEndpoint is required when Cosmos Persistence is enabled with AzureIdentity or ResourceToken authentication.");
        }

        if (options.EnableIntegratedCache && string.IsNullOrWhiteSpace(options.DedicatedGatewayEndpoint))
        {
            failures.Add("DedicatedGatewayEndpoint is required when integrated cache is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            failures.Add("DatabaseName is required when Cosmos Persistence is enabled.");
        }

        if (options.DefaultThroughput < 400)
        {
            failures.Add("DefaultThroughput must be at least 400 RU/s.");
        }

        if (options.ThroughputMode == VKCosmosThroughputMode.Autoscale && options.AutoscaleMaxThroughput < 1000)
        {
            failures.Add("AutoscaleMaxThroughput must be at least 1000 RU/s.");
        }

        if (options.MaxRetryAttemptsOnRateLimited < 0)
        {
            failures.Add("MaxRetryAttemptsOnRateLimited must be non-negative.");
        }

        if (options.MaxRetryIntervalSeconds < 1)
        {
            failures.Add("MaxRetryIntervalSeconds must be at least 1.");
        }
    }
}



