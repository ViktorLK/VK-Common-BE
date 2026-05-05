using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Validator for <see cref="VKInternalNetworkOptions"/>.
/// </summary>
internal sealed class InternalNetworkOptionsValidator : IValidateOptions<VKInternalNetworkOptions>
{
    private static readonly string Prefix = $"[{VKInternalNetworkOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKInternalNetworkOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (options.InternalCidrs is null || options.InternalCidrs.Count == 0)
        {
            failures.Add($"{Prefix}At least one internal CIDR range must be configured.");
        }
        else
        {
            foreach (var cidr in options.InternalCidrs)
            {
                if (!IsValidCidr(cidr))
                {
                    failures.Add($"{Prefix}Invalid CIDR format: '{cidr}'. Expected 'IP/Mask' (e.g., '10.0.0.0/8').");
                }
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static bool IsValidCidr(string cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr))
        {
            return false;
        }

        // Use Span for zero-allocation parsing (Rule 4)
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
