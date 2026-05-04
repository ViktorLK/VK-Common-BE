using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Grants access only when the request originates from within one of the configured
/// CIDR ranges. Also provides programmatic evaluation via <see cref="IVKInternalNetworkEvaluator"/>.
/// </summary>
internal sealed class InternalNetworkAuthorizationHandler(
    IVKIpAddressProvider ipAddressProvider,
    IOptions<VKAuthorizationOptions> globalOptions,
    IOptions<VKInternalNetworkOptions> networkOptions,
    ILogger<InternalNetworkAuthorizationHandler> logger)
    : AuthorizationHandler<VKInternalNetworkRequirement>, IVKInternalNetworkEvaluator
{
    private static string PolicyName => InternalNetworkConstants.FeatureName;

    private readonly IVKIpAddressProvider _ipAddressProvider = VKGuard.NotNull(ipAddressProvider);
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;
    private readonly VKInternalNetworkOptions _networkOptions = VKGuard.NotNull(networkOptions).Value;
    private readonly ILogger<InternalNetworkAuthorizationHandler> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKInternalNetworkRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await IsInternalNetworkAsync(context.User, allowedCidrs: requirement.AllowedCidrs)
            .ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> IsInternalNetworkAsync(
        ClaimsPrincipal? user = null,
        IPAddress? remoteIp = null,
        IReadOnlyList<string>? allowedCidrs = null,
        CancellationToken ct = default)
    {
        var userId = user?.Identity?.Name ?? VKBlocksConstants.SystemIdentity;

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user != null && user.IsSuperAdmin(_globalOptions))
        {
            _logger.LogInternalNetworkGranted(userId, remoteIp ?? IPAddress.None, $"{InternalNetworkConstants.FeatureName} (Bypassed)");
            return ValueTask.FromResult(VKResult.Success(true));
        }


        var sw = Stopwatch.StartNew();

        // 2. Resolve IP
        var ip = remoteIp ?? _ipAddressProvider.GetRemoteIpAddress();
        if (ip is null)
        {
            sw.RecordEvaluation(PolicyName, VKResult.Success(false));
            _logger.LogRemoteIpCouldNotBeDetermined(userId, PolicyName);
            return ValueTask.FromResult(VKResult.Success(false));
        }


        // 2. Normalise to IPv4 if necessary
        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        // 3. Match CIDRs
        var activeCidrs = allowedCidrs ?? _networkOptions.InternalCidrs;
        var isAllowed = activeCidrs?.Any(cidr => IsInCidr(ip, cidr)) == true;

        // 4. Trace & Record
        sw.RecordEvaluation(PolicyName, VKResult.Success(isAllowed));

        if (isAllowed)
        {
            _logger.LogInternalNetworkGranted(userId, ip, PolicyName);
            return ValueTask.FromResult(VKResult.Success(true));
        }

        _logger.LogInternalNetworkDenied(userId, ip, PolicyName);
        return ValueTask.FromResult(VKResult.Success(false));
    }

    private static bool IsInCidr(IPAddress ip, string cidr)
    {
        var span = cidr.AsSpan();
        var slashIndex = span.IndexOf('/');
        if (slashIndex == -1)
        {
            return false;
        }

        if (!IPAddress.TryParse(span[..slashIndex], out var network) ||
            !int.TryParse(span[(slashIndex + 1)..], out var prefixLength) ||
            prefixLength < 0 ||
            prefixLength > (network.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128))
        {
            return false;
        }

        if (ip.AddressFamily != network.AddressFamily)
        {
            return false;
        }

        // Use stackalloc to avoid heap allocation (Rule 4: ≤ 256 bytes)
        Span<byte> networkBytes = stackalloc byte[16];
        Span<byte> ipBytes = stackalloc byte[16];

        if (!network.TryWriteBytes(networkBytes, out var networkLength) ||
            !ip.TryWriteBytes(ipBytes, out var ipLength) ||
            networkLength != ipLength)
        {
            return false;
        }

        var activeNetworkBytes = networkBytes[..networkLength];
        var activeIpBytes = ipBytes[..ipLength];

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        if (fullBytes > networkLength)
        {
            return false;
        }

        for (var i = 0; i < fullBytes; i++)
        {
            if (activeNetworkBytes[i] != activeIpBytes[i])
            {
                return false;
            }
        }

        if (remainingBits > 0 && fullBytes < networkLength)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((activeNetworkBytes[fullBytes] & mask) != (activeIpBytes[fullBytes] & mask))
            {
                return false;
            }
        }

        return true;
    }
}
