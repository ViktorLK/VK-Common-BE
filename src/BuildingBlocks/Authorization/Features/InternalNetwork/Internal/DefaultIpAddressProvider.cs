using System.Net;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Authorization.Features.InternalNetwork.Internal;

/// <summary>
/// Default implementation of <see cref="IIpAddressProvider"/> using <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class DefaultIpAddressProvider(IHttpContextAccessor httpContextAccessor) : IIpAddressProvider
{
    /// <inheritdoc />
    public IPAddress? GetRemoteIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
    }
}
