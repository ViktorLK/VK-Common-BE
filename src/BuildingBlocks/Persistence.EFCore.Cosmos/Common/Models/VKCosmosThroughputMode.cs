namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Defines the provisioning throughput modes for Cosmos DB.
/// </summary>
public enum VKCosmosThroughputMode
{
    /// <summary>
    /// Fixed/Manual provisioned throughput.
    /// </summary>
    Manual,

    /// <summary>
    /// Autoscale provisioned throughput.
    /// </summary>
    Autoscale
}
