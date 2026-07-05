using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class InternalNetworkFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKInternalNetworkOptions options)
    {
        services.TryAddScoped<IVKIpAddressProvider, DefaultIpAddressProvider>();
        services.TryAddScoped<InternalNetworkAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, InternalNetworkAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKInternalNetworkEvaluator, InternalNetworkAuthorizationHandler>();

        services.AddOptions<AuthorizationOptions>()
            .Configure((AuthorizationOptions authOptions) =>
            {
                authOptions.AddPolicy(VKAuthorizationPolicies.InternalNetworkOnly, p =>
                    p.RequireVKInternalNetwork(options.InternalCidrs));
            });
    }

    static partial void ValidateFeatureCustom(VKInternalNetworkOptions options, List<string> failures)
    {
        if (options.InternalCidrs is null || options.InternalCidrs.Count == 0)
        {
            failures.Add("At least one internal CIDR range must be configured.");
        }
        else
        {
            foreach (var cidr in options.InternalCidrs)
            {
                if (!IsValidCidr(cidr))
                {
                    failures.Add($"Invalid CIDR format: '{cidr}'. Expected 'IP/Mask' (e.g., '10.0.0.0/8').");
                }
            }
        }
    }

    private static bool IsValidCidr(string cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr))
        {
            return false;
        }

        // Use Span for zero-allocation parsing (CS.04)
        ReadOnlySpan<char> span = cidr.AsSpan().Trim();
        int slashIndex = span.IndexOf('/');
        if (slashIndex == -1)
        {
            return false;
        }

        var ipPart = span[..slashIndex];
        var maskPart = span[(slashIndex + 1)..];

        if (!IPAddress.TryParse(ipPart, out var ip) ||
            !int.TryParse(maskPart, out var mask))
        {
            return false;
        }

        return ip.AddressFamily switch
        {
            AddressFamily.InterNetwork => mask is >= 0 and <= 32,
            AddressFamily.InterNetworkV6 => mask is >= 0 and <= 128,
            _ => false
        };
    }
}
