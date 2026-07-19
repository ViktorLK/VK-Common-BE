using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.Tracking.Diagnostics.Internal;
using VK.Blocks.Authentication.Tracking.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Tracking.Internal;

/// <summary>
/// Default implementation of <see cref="IVKLoginTracker"/> that logs login attempts.
/// </summary>
internal sealed class DefaultLoginTracker(ILogger<DefaultLoginTracker> logger) : IVKLoginTracker
{
    private readonly ILogger<DefaultLoginTracker> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public ValueTask<VKResult> TrackLoginAsync(VKLoginRecord record, CancellationToken ct = default)
    {
        VKGuard.NotNull(record);

        if (record.Success)
        {
            _logger.LogLoginSuccess(
                record.UserId,
                record.AuthenticationMethod,
                record.IpAddress,
                record.UserAgent,
                record.DeviceFingerprint ?? "None");
        }
        else
        {
            _logger.LogLoginFailure(
                record.UserId,
                record.AuthenticationMethod,
                record.FailureReason ?? "Unknown",
                record.IpAddress,
                record.UserAgent);
        }

        return ValueTask.FromResult(VKResult.Success());
    }
}
