using Azure.Core;
using Azure.Identity;
using VK.Blocks.Infrastructure.Azure.Abstractions;

namespace VK.Blocks.Infrastructure.Azure.Identity.Internal;

/// <summary>
/// Default implementation of the Azure credential provider.
/// </summary>
internal sealed class AzureCredentialProvider(VKAzureSharedOptions options) : IAzureCredentialProvider
{
    private readonly VKAzureSharedOptions _options = options;

    public TokenCredential GetCredential()
    {
        if (_options.Identity.UseManagedIdentity)
        {
            return new ManagedIdentityCredential();
        }

        if (!string.IsNullOrWhiteSpace(_options.Identity.ClientSecret))
        {
            return new ClientSecretCredential(
                _options.Identity.TenantId,
                _options.Identity.ClientId,
                _options.Identity.ClientSecret);
        }

        return new DefaultAzureCredential();
    }
}
