using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Options;

namespace VK.Blocks.Authentication.Factory;

/// <summary>
/// Factory for creating token validation parameters based on configuration options.
/// </summary>
public static class TokenValidationParametersFactory
{
    #region Public Methods

    /// <summary>
    /// Creates a new instance of <see cref="TokenValidationParameters"/> using the provided options.
    /// </summary>
    /// <param name="options">The JWT validation options.</param>
    /// <returns>Configured <see cref="TokenValidationParameters"/>.</returns>
    public static TokenValidationParameters Create(JwtValidationOptions options)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds)
        };
    }

    #endregion
}
