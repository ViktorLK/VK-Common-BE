using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Factory to provide custom JWT bearer events, primarily for token revocation verification.
/// </summary>
internal static class JwtEventsFactory
{
    /// <summary>
    /// Creates the JWT bearer events mapped to the standard authentication pipeline.
    /// </summary>
    /// <returns>A configured <see cref="JwtBearerEvents"/> instance.</returns>
    public static JwtBearerEvents CreateEvents()
    {
        return new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                IVKJwtAuthService? authService = context.HttpContext.RequestServices.GetService<IVKJwtAuthService>();
                if (authService is not null && context.Principal is not null)
                {
                    VKResult revocationResult = await authService.ValidateRevocationAsync(context.Principal, context.HttpContext.RequestAborted).ConfigureAwait(false);

                    if (revocationResult.IsFailure)
                    {
                        context.Fail(revocationResult.FirstError.Description);
                    }
                }
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                return AuthenticationResponseHelper.WriteUnauthorizedResponseAsync(
                    context.HttpContext,
                    JwtConstants.DefaultUnauthorizedDetail);
            },
            OnAuthenticationFailed = context =>
            {
                // Attach a specific header if the token was rejected solely due to expiration
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append(
                        JwtConstants.TokenExpiredHeader,
                        JwtConstants.HeaderTrueValue);
                }
                return Task.CompletedTask;
            }
        };
    }
}
