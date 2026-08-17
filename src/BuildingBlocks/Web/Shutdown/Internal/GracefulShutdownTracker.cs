using System.Threading;

namespace VK.Blocks.Web.Shutdown.Internal;

/// <summary>
/// Tracks the number of active HTTP requests in-flight.
/// </summary>
internal sealed class GracefulShutdownTracker
{
    private int _activeRequestCount;

    /// <summary>
    /// Gets the number of active requests currently processing.
    /// </summary>
    public int ActiveRequests => _activeRequestCount;

    /// <summary>
    /// Increments the active request count.
    /// </summary>
    public void Increment()
    {
        Interlocked.Increment(ref _activeRequestCount);
    }

    /// <summary>
    /// Decrements the active request count.
    /// </summary>
    public void Decrement()
    {
        Interlocked.Decrement(ref _activeRequestCount);
    }
}
