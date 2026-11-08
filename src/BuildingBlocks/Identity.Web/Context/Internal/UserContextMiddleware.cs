using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Identity.Context.Internal;

namespace VK.Blocks.Identity.Web.Context.Internal;

/// <summary>
/// Middleware that extracts user claims from <see cref="HttpContext.User"/> and pushes an active <see cref="IVKUserContext"/>
/// into the ambient execution context flow (<see cref="VKAmbientExecutionContext"/>).
/// Follows AP.01, AP.03, AP.06, CS.03.
/// </summary>
internal sealed class UserContextMiddleware(
    RequestDelegate next,
    IOptions<VKIdentityWebOptions> options)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);
    private readonly VKIdentityWebOptions _options = VKGuard.NotNull(options).Value;

    public async Task InvokeAsync(
        HttpContext context,
        IVKAmbientContextAccessor ambientAccessor)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(ambientAccessor);

        var principal = context.User;
        var userId = ExtractUserId(principal);

        var userContext = new DefaultUserContext(
            userId: userId,
            principal: principal);

        using (ambientAccessor.BeginScope(userContext))
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    private VKUserId ExtractUserId(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return VKUserId.Anonymous;
        }

        var claimValue = principal.FindFirst(_options.UserIdClaimType)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return VKUserId.FromNullable(claimValue);
    }
}
