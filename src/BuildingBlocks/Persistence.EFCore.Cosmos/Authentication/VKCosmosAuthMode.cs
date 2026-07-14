namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Defines the authentication modes supported by VK Cosmos DB persistence.
/// </summary>
public enum VKCosmosAuthMode
{
    /// <summary>
    /// Authenticate using a standard Connection String.
    /// </summary>
    ConnectionString,

    /// <summary>
    /// Authenticate using Azure Identity token credentials.
    /// </summary>
    AzureIdentity,

    /// <summary>
    /// Authenticate using dynamic user resource tokens.
    /// </summary>
    ResourceToken
}
