using System.Net;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides access to the remote IP address of the current request.
/// </summary>
public interface IVKIpAddressProvider
{
    /// <summary>
    /// Gets the remote IP address.
    /// </summary>
    /// <returns>The remote IP, or null if it cannot be determined.</returns>
    IPAddress? GetRemoteIpAddress();
}
