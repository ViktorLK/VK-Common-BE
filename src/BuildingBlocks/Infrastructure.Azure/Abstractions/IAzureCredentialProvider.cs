using Azure.Core;

namespace VK.Blocks.Infrastructure.Azure.Abstractions;



/// <summary>
/// A provider for Azure credentials.
/// </summary>
public interface IAzureCredentialProvider
{
    /// <summary>
    /// Gets the token credential for Azure service authentication.
    /// </summary>
    TokenCredential GetCredential();
}
