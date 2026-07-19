using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.Common.Protocols;
using VK.Blocks.Authentication.Cookies.Protocols;
using VK.Blocks.Authentication.Jwt;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKSloHandler"/> that coordinates logout across Cookie and JWT sub-systems.
/// </summary>
internal sealed class DefaultSloHandler(IServiceProvider serviceProvider) : IVKSloHandler
{
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

    /// <inheritdoc />
    public async ValueTask<VKResult> SignOutUserGloballyAsync(string userId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);

        // 1. Revoke active Cookie sessions if Cookies slice is registered
        var sessionStore = _serviceProvider.GetService<IVKSessionStore>();
        if (sessionStore is not null)
        {
            await sessionStore.RevokeUserSessionsAsync(userId, ct).ConfigureAwait(false);
        }

        // 2. Revoke active JWT refresh tokens if JWT slice is registered
        var jwtRevocation = _serviceProvider.GetService<IVKJwtRevocationService>();
        if (jwtRevocation is not null)
        {
            await jwtRevocation.RevokeAllUserTokensAsync(userId, null, ct).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
