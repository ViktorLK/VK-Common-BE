using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Abstractions;

namespace VK.Blocks.Authentication.Validation;

/// <summary>
/// Factory to provide custom JWT bearer events, primarily for token revocation verification.
/// </summary>
internal static class JwtBearerEventsFactory
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
                var blacklist = context.HttpContext.RequestServices.GetService<ITokenBlacklist>();
                if (blacklist != null)
                {
                    // Attempt to extract the "jti" claim for explicit token blacklist checking
                    var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

                    if (string.IsNullOrEmpty(jti))
                    {
                        // Fallback generic check if framework maps things differently
                        jti = context.Principal?.FindFirst("jti")?.Value;
                    }

                    // Check if the token has been explicitly revoked in the blacklist
                    if (!string.IsNullOrEmpty(jti) && await blacklist.IsRevokedAsync(jti, context.HttpContext.RequestAborted))
                    {
                        context.Fail("Token has been revoked.");
                    }
                }
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new { error = "You are not authorized." });
                return context.Response.WriteAsync(result);
            },
            OnAuthenticationFailed = context =>
            {
                // Attach a specific header if the token was rejected solely due to expiration
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    }

    #endregion
}
