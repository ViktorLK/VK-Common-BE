using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authentication.Contracts;
using VK.Blocks.Core.Security;

namespace VK.Blocks.Authentication.Diagnostics;

/// <summary>
/// Provides authentication-specific security metadata.
/// </summary>
public sealed class AuthenticationMetadataProvider : ISecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => nameof(AuthenticationBlock);

    /// <inheritdoc />
    public async ValueTask<SecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // 1. Endpoint specific metadata (Source Generated)
        var metadata = AuthenticationDiagnostics.GetAuthenticationMetadata();
        var endpointEntries = metadata.Select(kvp => new SecurityMetadataEntry(
            Key: kvp.Key,
            Metadata: kvp.Value,
            Module: Module)).ToList();

        // 2. Catalogs (Authentication Schemes)
        // SUGGEST: Use C# 12 collection expression [...]
        Dictionary<string, object> catalogs = new Dictionary<string, object>
        {
            ["Schemes"] = await AuthenticationDiagnostics.GetRegisteredSchemesAsync(serviceProvider).ConfigureAwait(false)
        };

        return new SecurityTopology
        {
            Module = Module,
            Endpoints = endpointEntries,
            Catalogs = catalogs
        };
    }
}



