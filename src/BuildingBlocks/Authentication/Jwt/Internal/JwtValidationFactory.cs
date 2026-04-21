using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Factory for creating JWT-specific token validation parameters based on configuration options.
/// </summary>
internal static class JwtValidationFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="TokenValidationParameters"/> using the provided options.
    /// </summary>
    /// <param name="options">The JWT options.</param>
    /// <returns>Configured <see cref="TokenValidationParameters"/>.</returns>
    public static TokenValidationParameters Create(VKJwtOptions options)
    {
        var parameters = new TokenValidationParameters
        {
            // Core Security Checks
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,

            // Configuration Mapping
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,

            // Time Management
            ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds),

            // Identity Mapping (VK Standards)
            NameClaimType = VKClaimConstants.Name,
            RoleClaimType = VKClaimConstants.Role
        };

        if (options.AuthMode == VKJwtAuthMode.Symmetric)
        {
            parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        }

        return parameters;
    }
}









