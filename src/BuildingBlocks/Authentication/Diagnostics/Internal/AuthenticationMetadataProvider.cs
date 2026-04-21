using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Diagnostics.Internal;

/// <summary>
/// Provides authentication-specific security metadata.
/// </summary>
internal sealed class AuthenticationMetadataProvider : IVKSecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => nameof(AuthenticationBlock);

    /// <inheritdoc />
    public async ValueTask<VKSecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // 1. Endpoint specific metadata (Source Generated)
        IReadOnlyDictionary<string, VKEndpointAuthInfo> metadata = AuthenticationDiagnostics.GetAuthenticationMetadata();
        var endpointEntries = metadata.Select(kvp => new VKSecurityMetadataEntry(
            Key: kvp.Key,
            Metadata: kvp.Value,
            Module: Module)).ToList();

        // 2. Catalogs (Authentication Schemes)
        var catalogs = new Dictionary<string, object>
        {
            ["Schemes"] = await AuthenticationDiagnostics.GetRegisteredSchemesAsync(serviceProvider).ConfigureAwait(false)
        };

        return new VKSecurityTopology
        {
            Module = Module,
            Endpoints = endpointEntries,
            Catalogs = catalogs
        };
    }
}
