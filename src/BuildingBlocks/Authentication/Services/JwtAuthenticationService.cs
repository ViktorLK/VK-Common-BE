using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Abstractions.Contracts;
using VK.Blocks.Authentication.Claims;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Factory;
using VK.Blocks.Authentication.Options;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Services;

/// <summary>
/// Implements <see cref="IAuthenticationService"/> to validate JWT tokens and map them to <see cref="AuthUser"/>.
/// </summary>
public sealed class JwtAuthenticationService(
    IOptionsMonitor<VKAuthenticationOptions> optionsFallback,
    ILogger<JwtAuthenticationService> logger,
    ITokenBlacklist blacklist) : IAuthenticationService
{
    #region Fields

    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<Result<AuthUser>> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        using var activity = AuthenticationDiagnostics.Source.StartActivity("JwtAuthenticationService.AuthenticateAsync");

        if (string.IsNullOrWhiteSpace(token))
        {
            AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
            return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.EmptyToken);
        }

        var authOptions = optionsFallback.CurrentValue;

        if (authOptions.Jwt == null || string.IsNullOrEmpty(authOptions.Jwt.SecretKey))
        {
            logger.LogError("JWT Validation options are not configured properly.");
            AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
            return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.ConfigurationError);
        }

        try
        {
            var validationParameters = TokenValidationParametersFactory.Create(authOptions.Jwt);

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
                return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.InvalidFormat);
            }

            // 1. User-level revocation check
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? principal.FindFirst(VKClaimTypes.UserId)?.Value;

            if (!string.IsNullOrEmpty(userId) && await blacklist.IsUserRevokedAsync(userId, cancellationToken).ConfigureAwait(false))
            {
                AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
                return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.Revoked);
            }

            // 2. Token-level (jti) revocation check
            if (jwtToken.Id is { Length: > 0 } jti)
            {
                if (await blacklist.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false))
                {
                    AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
                    return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.Revoked);
                }
            }

            AuthenticationDiagnostics.RecordAuthAttempt("jwt", true);
            activity?.SetTag("auth.user.id", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst(VKClaimTypes.UserId)?.Value);
            return Result.Success(MapToAuthUser(principal));
        }
        catch (SecurityTokenExpiredException ex)
        {
            logger.LogWarning(ex, "Failed to authenticate token because it is expired.");
            AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
            return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.Expired);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to authenticate token.");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AuthenticationDiagnostics.RecordAuthAttempt("jwt", false);
            return Result.Failure<AuthUser>(AuthenticationErrors.Jwt.Invalid);
        }
    }

    #endregion

    #region Private Methods

    private static AuthUser MapToAuthUser(ClaimsPrincipal principal)
    {
        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? principal.FindFirst(VKClaimTypes.UserId)?.Value;

        var username = principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? principal.FindFirst("preferred_username")?.Value;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var claimsDict = new Dictionary<string, string>();
        foreach (var claim in principal.Claims)
        {
            // Prefer TryAdd to avoid duplicate key exceptions
            claimsDict.TryAdd(claim.Type, claim.Value);
        }

        return new AuthUser
        {
            Id = id ?? string.Empty,
            Username = username ?? string.Empty,
            Email = email,
            Roles = roles,
            Claims = claimsDict
        };
    }

    #endregion
}
