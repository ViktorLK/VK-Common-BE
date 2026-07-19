using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Tracking.Protocols;
using VK.Blocks.Authentication.Tracking.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Tracking.Internal;

/// <summary>
/// A helper class to capture client details from HttpContext and record login state tracking.
/// </summary>
internal sealed class LoginTrackingHelper(
    IVKLoginTracker loginTracker,
    IOptions<VKTrackingOptions> options,
    ILogger<LoginTrackingHelper> logger)
{
    private readonly IVKLoginTracker _loginTracker = VKGuard.NotNull(loginTracker);
    private readonly VKTrackingOptions _options = VKGuard.NotNull(options).Value;
    private readonly ILogger<LoginTrackingHelper> _logger = VKGuard.NotNull(logger);

    /// <summary>
    /// Records a login attempt by extracting IP address, User-Agent, and device fingerprint from HttpContext.
    /// </summary>
    public async ValueTask RecordLoginAsync(
        HttpContext httpContext,
        string userId,
        string authMethod,
        bool success,
        string? failureReason = null)
    {
        VKGuard.NotNull(httpContext);
        VKGuard.NotNullOrWhiteSpace(userId);

        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string ua = httpContext.Request.Headers.UserAgent.ToString() ?? "unknown";

        string? fingerprint = null;
        if (httpContext.Request.Headers.TryGetValue(_options.FingerprintHeaderName, out var values))
        {
            fingerprint = values.ToString();
        }
        else if (_options.WarnOnMissingFingerprint && success)
        {
            _logger.LogMissingFingerprint(_options.FingerprintHeaderName, userId);
        }

        var record = new VKLoginRecord
        {
            UserId = userId,
            AuthenticationMethod = authMethod,
            IpAddress = ip,
            UserAgent = ua,
            DeviceFingerprint = fingerprint,
            Timestamp = DateTimeOffset.UtcNow,
            Success = success,
            FailureReason = failureReason
        };

        await _loginTracker.TrackLoginAsync(record, httpContext.RequestAborted).ConfigureAwait(false);
    }
}
