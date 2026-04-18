using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Comprehensive validator for <see cref="VKAuthorizationOptions"/> configuration.
/// </summary>
public sealed class VKAuthorizationOptionsValidator : IValidateOptions<VKAuthorizationOptions>
{
    private const string Prefix = "[Authorization Check] ";

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAuthorizationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        // 1. Claim Type Validation
        ValidateRequiredString(options.RoleClaimType, nameof(options.RoleClaimType), failures);
        ValidateRequiredString(options.PermissionClaimType, nameof(options.PermissionClaimType), failures);
        ValidateRequiredString(options.TenantClaimType, nameof(options.TenantClaimType), failures);
        ValidateRequiredString(options.RankClaimType, nameof(options.RankClaimType), failures);

        // 2. Working Hours Validation
        if (options.WorkStart >= options.WorkEnd)
        {
            failures.Add($"{Prefix}WorkStart ({options.WorkStart}) must be earlier than WorkEnd ({options.WorkEnd}).");
        }

        // 3. CIDR / Network Validation
        if (options.InternalCidrs is null || options.InternalCidrs.Count == 0)
        {
            failures.Add($"{Prefix}At least one internal CIDR range must be configured (InternalCidrs).");
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

    #region Private Helpers

    private static void ValidateRequiredString(string value, string propertyName, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{Prefix}{propertyName} cannot be null or whitespace.");
        }
    }

    private static bool IsValidCidr(string cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr))
        {
            return false;
        }

        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        var ipPart = parts[0];
        var maskPart = parts[1];

        if (!IPAddress.TryParse(ipPart, out var ip))
        {
            return false;
        }
        if (!int.TryParse(maskPart, out var mask))
        {
            return false;
        }

        // IPv4 mask 0-32, IPv6 mask 0-128
        return ip.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => mask >= 0 && mask <= 32,
            System.Net.Sockets.AddressFamily.InterNetworkV6 => mask >= 0 && mask <= 128,
            _ => false
        };
    }

    #endregion
}
