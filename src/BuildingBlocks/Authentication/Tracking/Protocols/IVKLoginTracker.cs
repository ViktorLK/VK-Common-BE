using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Tracking.Protocols;

/// <summary>
/// Defines a contract for tracking and auditing user login events.
/// </summary>
public interface IVKLoginTracker
{
    /// <summary>
    /// Tracks and records a login attempt.
    /// </summary>
    /// <param name="record">The login record details.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result indicating success or failure of the tracking operation.</returns>
    ValueTask<VKResult> TrackLoginAsync(VKLoginRecord record, CancellationToken ct = default);
}
