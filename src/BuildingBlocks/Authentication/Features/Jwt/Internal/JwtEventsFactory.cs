using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.Features.Jwt.Internal;

/// <summary>
/// Factory to provide custom JWT bearer events, primarily for token revocation verification.
/// </summary>
internal static class JwtEventsFactory
{
    #region Public Methods

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
                var authService = context.HttpContext.RequestServices.GetService<IJwtAuthenticationService>();
                if (authService is not null && context.Principal is not null)
                {
                    var revocationResult = await authService.ValidateRevocationAsync(context.Principal, context.HttpContext.RequestAborted).ConfigureAwait(false);

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

    #endregion
}
