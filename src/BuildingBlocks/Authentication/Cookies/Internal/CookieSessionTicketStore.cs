using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Cookies.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Cookies.Internal;

/// <summary>
/// Implements ASP.NET Core <see cref="ITicketStore"/> by mapping tickets to an underlying <see cref="IVKSessionStore"/>.
/// </summary>
internal sealed class CookieSessionTicketStore(
    IVKSessionStore sessionStore,
    IOptions<VKCookieOptions> options) : ITicketStore
{
    private readonly IVKSessionStore _sessionStore = VKGuard.NotNull(sessionStore);
    private readonly VKCookieOptions _options = VKGuard.NotNull(options).Value;
    private readonly TicketSerializer _serializer = TicketSerializer.Default;

    /// <inheritdoc />
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        VKGuard.NotNull(ticket);

        byte[] serialized = _serializer.Serialize(ticket);
        string base64 = Convert.ToBase64String(serialized);

        string userId = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? ticket.Principal.FindFirst("sub")?.Value
                        ?? "unknown";

        DateTimeOffset expiresAt = ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(_options.ExpireMinutes);

        var result = await _sessionStore.RegisterSessionAsync(
            userId,
            base64,
            expiresAt,
            _options.MaxConcurrentSessions,
            _options.KickOldestSession).ConfigureAwait(false);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.FirstError.Description);
        }

        return result.Value;
    }

    /// <inheritdoc />
    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(ticket);

        byte[] serialized = _serializer.Serialize(ticket);
        string base64 = Convert.ToBase64String(serialized);
        DateTimeOffset expiresAt = ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(_options.ExpireMinutes);

        await _sessionStore.UpdateSessionTicketAsync(key, base64, expiresAt).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        string? base64 = await _sessionStore.GetSessionTicketAsync(key).ConfigureAwait(false);
        if (string.IsNullOrEmpty(base64))
        {
            return null;
        }

        byte[] serialized = Convert.FromBase64String(base64);
        return _serializer.Deserialize(serialized);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        await _sessionStore.RevokeSessionAsync(key).ConfigureAwait(false);
    }
}
