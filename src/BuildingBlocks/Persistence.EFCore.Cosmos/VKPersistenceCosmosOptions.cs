using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Represents the configuration options for Cosmos DB persistence.
/// </summary>
public sealed partial record VKPersistenceCosmosOptions
{
    /// <summary>
    /// Gets the connection string to Cosmos DB.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; init; } = "VKDb";

    /// <summary>
    /// Gets the default throughput (RU/s).
    /// </summary>
    public int DefaultThroughput { get; init; } = 400;

    /// <summary>
    /// Gets the throughput provisioning mode.
    /// Default is <see cref="CosmosThroughputMode.Manual"/>.
    /// </summary>
    public VKCosmosThroughputMode ThroughputMode { get; init; } = VKCosmosThroughputMode.Manual;

    /// <summary>
    /// Gets the maximum throughput for Autoscale mode (RU/s). Minimum is 1000.
    /// Default is 4000.
    /// </summary>
    public int AutoscaleMaxThroughput { get; init; } = 4000;

    /// <summary>
    /// Gets the consistency level override. If null, the Cosmos DB account's default consistency is used.
    /// </summary>
    public VKCosmosConsistencyLevel? ConsistencyLevel { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable session token propagation for read-your-own-writes guarantees.
    /// Default is <c>true</c>.
    /// </summary>
    public bool EnableSessionTokenPropagation { get; init; } = true;

    /// <summary>
    /// Gets preferred regions for multi-region failover (ordered by priority).
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<string> PreferredRegions { get; init; } = System.Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether to enable multi-region writes.
    /// Default is <c>false</c>.
    /// </summary>
    public bool EnableMultiRegionWrites { get; init; } = false;

    /// <summary>
    /// Gets the maximum retry attempts on rate-limited (429) requests.
    /// Default is 3.
    /// </summary>
    public int MaxRetryAttemptsOnRateLimited { get; init; } = 3;

    /// <summary>
    /// Gets the maximum retry interval in seconds for 429 responses.
    /// Default is 5.
    /// </summary>
    public int MaxRetryIntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Gets a value indicating whether Analytical Store awareness is enabled (informational only).
    /// </summary>
    public bool AnalyticalStoreAwareness { get; init; } = false;

    /// <summary>
    /// Gets the connection mode (Direct or Gateway). Default is Direct.
    /// </summary>
    public VKCosmosConnectionMode ConnectionMode { get; init; } = VKCosmosConnectionMode.Direct;

    /// <summary>
    /// Gets a value indicating whether bulk execution mode is enabled for high-throughput operations.
    /// </summary>
    public bool EnableBulkExecution { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether the Cosmos DB integrated cache (dedicated gateway) is enabled.
    /// </summary>
    public bool EnableIntegratedCache { get; init; } = false;

    /// <summary>
    /// Gets the dedicated gateway endpoint for integrated cache. Required when EnableIntegratedCache is true.
    /// </summary>
    public string? DedicatedGatewayEndpoint { get; init; }

    /// <summary>
    /// Gets the authentication mode. Default is ConnectionString.
    /// </summary>
    public VKCosmosAuthMode AuthMode { get; init; } = VKCosmosAuthMode.ConnectionString;

    /// <summary>
    /// Gets the Cosmos DB account endpoint URI. Required for AzureIdentity and ResourceToken modes.
    /// </summary>
    public string? AccountEndpoint { get; init; }
}




