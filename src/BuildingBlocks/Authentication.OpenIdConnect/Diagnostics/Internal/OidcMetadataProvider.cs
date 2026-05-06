using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Diagnostics.Internal;

/// <summary>
/// Provides security metadata for the OpenIdConnect block.
/// Complies with Rule 19.
/// </summary>
internal sealed class OidcMetadataProvider : IVKSecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => nameof(VKOidcBlock);

    /// <inheritdoc />
    public ValueTask<VKSecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // OIDC typically doesn't have its own endpoint metadata in the same way the main Auth block does,
        // but it could expose registered providers.
        return new ValueTask<VKSecurityTopology>(new VKSecurityTopology
        {
            Module = Module,
            Endpoints = [],
            Catalogs = new Dictionary<string, object>
            {
                ["Provider"] = "OpenID Connect"
            }
        });
    }
}
