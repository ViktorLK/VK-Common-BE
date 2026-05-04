using System.Net;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIpAddressProvider"/> using <see cref="IHttpContextAccessor"/>.
/// </summary>
internal sealed class DefaultIpAddressProvider(IHttpContextAccessor httpContextAccessor) : IVKIpAddressProvider
{
    /// <inheritdoc />
    public IPAddress? GetRemoteIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
    }
}
