using System.Net;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Source-generated logging for the InternalNetwork feature.
/// Provides detailed diagnostic context for IP-based authorization.
/// </summary>
internal static partial class InternalNetworkLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Internal network check succeeded for user {UserId}. Source IP: {RemoteIp}. Policy: {PolicyName}.")]
    public static partial void LogInternalNetworkGranted(
        this ILogger logger,
        string userId,
        IPAddress? remoteIp,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Internal network check denied for user {UserId}. IP {RemoteIp} is not in an allowed range. Policy: {PolicyName}.")]
    public static partial void LogInternalNetworkDenied(
        this ILogger logger,
        string userId,
        IPAddress? remoteIp,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Internal network check failed for user {UserId}. Remote IP address could not be determined. Policy: {PolicyName}.")]
    public static partial void LogRemoteIpCouldNotBeDetermined(
        this ILogger logger,
        string userId,
        string policyName);
}
