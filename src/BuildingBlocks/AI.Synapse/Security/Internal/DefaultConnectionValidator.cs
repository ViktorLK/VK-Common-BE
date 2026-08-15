using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Security.Internal;

/// <summary>
/// Default allowlist-based connection validator for multi-tenant SSRF and BYOK guardrails.
/// </summary>
internal sealed class DefaultConnectionValidator : IVKConnectionValidator
{
    private readonly ConcurrentDictionary<VKTenantId, HashSet<string>> _endpointAllowlists = new();
    private readonly ConcurrentDictionary<VKTenantId, HashSet<string>> _byokAllowlists = new();

    public VKResult ValidateEndpoint(VKTenantId tenantId, string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return VKResult.Success(); // Null means use tenant default
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return VKResult.Failure(VKAISynapseErrors.InvalidEndpoint);
        }

        // Prevent local/loopback/internal SSRF targets by default unless explicitly allowed
        if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.StartsWith("127.") || uri.Host.StartsWith("10.") || uri.Host.StartsWith("192.168."))
        {
            if (!_endpointAllowlists.TryGetValue(tenantId, out var allowlist) || !allowlist.Contains(endpoint))
            {
                return VKResult.Failure(VKAISynapseErrors.UnauthorizedConnection);
            }
        }

        return VKResult.Success();
    }

    public VKResult ValidateApiKey(VKTenantId tenantId, VKSensitiveString? apiKey)
    {
        if (apiKey is null || apiKey.Value.IsEmpty || string.IsNullOrWhiteSpace(apiKey.Value.Reveal()))
        {
            return VKResult.Success(); // Null means use tenant default Key
        }

        // If tenant has BYOK restrictions configured, check membership
        if (_byokAllowlists.TryGetValue(tenantId, out var allowlist) && allowlist.Count > 0)
        {
            if (!allowlist.Contains(apiKey.Value.Reveal()))
            {
                return VKResult.Failure(VKAISynapseErrors.UnauthorizedConnection);
            }
        }

        return VKResult.Success();
    }
}
