using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Authorization.Diagnostics.Models;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.Authorization.Diagnostics;

/// <summary>
/// Provides authorization-specific security metadata.
/// </summary>
public sealed class AuthorizationMetadataProvider : ISecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => nameof(AuthorizationBlock);

    /// <inheritdoc />
    public async ValueTask<SecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // 1. Endpoint specific metadata (Source Generated)
        var metadata = AuthorizationDiagnostics.GetAuthorizationMetadata();
        var endpointEntries = metadata.Select(kvp => new SecurityMetadataEntry(
            Key: kvp.Key,
            Metadata: kvp.Value,
            Module: Module)).ToList();

        // 2. Catalogs (Permissions and Handlers)
        var catalogs = new Dictionary<string, object>
        {
            ["Permissions"] = PermissionsCatalog.All,
            ["Handlers"] = await AuthorizationDiagnostics.GetRegisteredHandlersAsync(serviceProvider).ConfigureAwait(false)
        };

        return new SecurityTopology
        {
            Module = Module,
            Endpoints = endpointEntries,
            Catalogs = catalogs
        };
    }
}
