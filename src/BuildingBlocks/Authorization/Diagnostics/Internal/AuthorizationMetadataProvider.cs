using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Diagnostics.Internal;

/// <summary>
/// Provides authorization-specific security metadata.
/// </summary>
internal sealed class AuthorizationMetadataProvider : IVKSecurityMetadataProvider
{
    /// <inheritdoc />
    public string Module => VKAuthorizationBlock.BlockName;

    /// <inheritdoc />
    public async ValueTask<VKSecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // 1. Endpoint specific metadata (Source Generated)
        var metadata = AuthorizationDiagnostics.GetAuthorizationMetadata();
        var endpointEntries = metadata.Select(kvp => new VKSecurityMetadataEntry(
            Key: kvp.Key,
            Metadata: kvp.Value,
            Module: Module)).ToList();

        // 2. Catalogs (Permissions and Handlers)
        var catalogs = new Dictionary<string, object>
        {
            ["Permissions"] = PermissionsCatalog.All,
            ["Handlers"] = await AuthorizationDiagnostics.GetRegisteredHandlersAsync(serviceProvider).ConfigureAwait(false)
        };

        return new VKSecurityTopology
        {
            Module = Module,
            Endpoints = endpointEntries,
            Catalogs = catalogs
        };
    }
}
