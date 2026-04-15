using System.Collections.Generic;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Authentication.OpenIdConnect.DependencyInjection;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.Authentication.OpenIdConnect.Diagnostics;

/// <summary>
/// Provides OpenID Connect specific security metadata.
/// </summary>
public sealed class OidcMetadataProvider : ISecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => nameof(OidcBlock);

    /// <inheritdoc />
    public ValueTask<SecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var catalogs = new Dictionary<string, object>
        {
            ["Providers"] = VKOidcGeneratedMetadata.AllProviders
        };

        return ValueTask.FromResult(new SecurityTopology
        {
            Module = Module,
            Endpoints = [],
            Catalogs = catalogs
        });
    }
}
