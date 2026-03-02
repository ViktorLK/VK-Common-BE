using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Abstractions.Contracts;

namespace VK.Blocks.Authentication.Extensions;

/// <summary>
/// Provides extension methods and utility classes for authentication configuration.
/// </summary>
internal class AuthenticationExtensions
{
    #region Public Methods

    /// <summary>
    /// Creates configured <see cref="JwtBearerEvents"/> instances tailored for the VK system.
    /// </summary>
    /// <param name="blacklist">The token blacklist service.</param>
    /// <returns>A configured <see cref="JwtBearerEvents"/> object.</returns>
    public static JwtBearerEvents CreateVKJwtBearerEvents(
        ITokenBlacklist blacklist)
    {
        return new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                // Verify if the token ID is present and revoked
                if (!string.IsNullOrEmpty(jti) && await blacklist.IsRevokedAsync(jti).ConfigureAwait(false))
                {
                    context.Fail("Token has been revoked");
                }
            },
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };
    }

    #endregion

}
