namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Defines the connection mode used by the Cosmos DB client.
/// </summary>
public enum VKCosmosConnectionMode
{
    /// <summary>
    /// Direct mode (default, high-performance connection straight to replica nodes).
    /// </summary>
    Direct,

    /// <summary>
    /// Gateway mode (uses HTTPS routing via gateway, suitable for restrictive networks).
    /// </summary>
    Gateway
}
