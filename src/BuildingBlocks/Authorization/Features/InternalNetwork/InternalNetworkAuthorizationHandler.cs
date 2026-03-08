using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Grants access only when the request originates from within one of the configured
/// CIDR ranges specified by <see cref="InternalNetworkRequirement"/>.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Security Note:</strong> This handler relies on <c>HttpContext.Connection.RemoteIpAddress</c>.
/// If the application is hosted behind a reverse proxy or load balancer, you <strong>must</strong>
/// configure the <c>ForwardedHeadersMiddleware</c> in the application pipeline (e.g. <c>UseForwardedHeaders()</c>)
/// to ensure the original client IP is correctly populated, otherwise IP spoofing is possible.
/// </para>
/// </remarks>
public sealed class InternalNetworkAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<InternalNetworkRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalNetworkRequirement requirement)
    {
        var remoteIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        if (remoteIp is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "Remote IP address could not be determined."));
            return Task.CompletedTask;
        }

        // Normalise to IPv4 if the address is an IPv4-mapped IPv6 address.
        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        var allowed = requirement.AllowedCidrs.Any(cidr => IsInCidr(remoteIp, cidr));

        if (allowed)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this,
                $"IP {remoteIp} is not in an allowed network range."));
        }

        return Task.CompletedTask;
    }

    private static bool IsInCidr(IPAddress ip, string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!IPAddress.TryParse(parts[0], out var network))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        // Ensure both addresses are in the same family.
        if (ip.AddressFamily != network.AddressFamily)
        {
            return false;
        }

        var networkBytes = network.GetAddressBytes();
        var ipBytes = ip.GetAddressBytes();

        if (networkBytes.Length != ipBytes.Length)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != ipBytes[i])
            {
                return false;
            }
        }

        for (var i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != ipBytes[i])
            {
                return false;
            }
        }

        if (remainingBits > 0)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((networkBytes[fullBytes] & mask) != (ipBytes[fullBytes] & mask))
            {
                return false;
            }
        }

        return true;
    }
}


