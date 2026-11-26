using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Security.Internal;

/// <summary>
/// Default allowlist-based connection validator for SSRF and BYOK guardrails.
/// </summary>
internal sealed class DefaultConnectionValidator : IVKConnectionValidator
{
    private readonly HashSet<string> _endpointAllowlist = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _byokAllowlist = new(StringComparer.Ordinal);

    public VKResult ValidateEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return VKResult.Success(); // Null means use default
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return VKResult.Failure(VKAISynapseErrors.InvalidEndpoint);
        }

        // Prevent local/loopback/internal/link-local SSRF targets by default unless explicitly allowed
        if (uri.IsLoopback
            || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.StartsWith("127.", StringComparison.Ordinal)
            || uri.Host.StartsWith("10.", StringComparison.Ordinal)
            || uri.Host.StartsWith("192.168.", StringComparison.Ordinal)
            || uri.Host.StartsWith("169.254.", StringComparison.Ordinal)
            || IsPrivate172(uri.Host))
        {
            if (!_endpointAllowlist.Contains(endpoint))
            {
                return VKResult.Failure(VKAISynapseErrors.UnauthorizedConnection);
            }
        }

        return VKResult.Success();
    }

    public VKResult ValidateApiKey(VKSensitiveString? apiKey)
    {
        if (apiKey is null || apiKey.Value.IsEmpty || string.IsNullOrWhiteSpace(apiKey.Value.Reveal()))
        {
            return VKResult.Success(); // Null means use default Key
        }

        // If BYOK restrictions are configured, check membership
        if (_byokAllowlist.Count > 0 && !_byokAllowlist.Contains(apiKey.Value.Reveal()))
        {
            return VKResult.Failure(VKAISynapseErrors.UnauthorizedConnection);
        }

        return VKResult.Success();
    }

    private static bool IsPrivate172(string host)
    {
        if (!host.StartsWith("172.", StringComparison.Ordinal))
        {
            return false;
        }

        var parts = host.Split('.');
        return parts.Length >= 2 && int.TryParse(parts[1], out var second) && second is >= 16 and <= 31;
    }
}
