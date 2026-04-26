using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Implements <see cref="IVKJwtAuthService"/> to validate JWT tokens and map them to <see cref="AuthenticatedUser"/>.
/// Uses the modern <see cref="JsonWebTokenHandler"/> for high-performance validation.
/// </summary>
internal sealed class JwtAuthenticationService(
    IOptions<VKJwtOptions> options,
    ILogger<JwtAuthenticationService> logger,
    IVKJwtRevocationProvider revocationProvider) : IVKJwtAuthService
{
    private readonly VKJwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly Dictionary<string, bool> _revocationCache = [];

    /// <inheritdoc />
    public async ValueTask<VKResult<VKAuthenticatedUser>> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        using Activity? activity = AuthenticationDiagnostics.StartJwtValidation();

        if (string.IsNullOrWhiteSpace(token))
        {
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, VKJwtErrors.EmptyToken.Code);
            return VKResult.Failure<VKAuthenticatedUser>(VKJwtErrors.EmptyToken);
        }

        try
        {
            VKJwtOptions jwtOptions = _options;

            if (string.IsNullOrEmpty(jwtOptions.SecretKey))
            {
                logger.LogOptionsNotConfigured();
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, VKJwtErrors.ConfigurationError.Code);
                return VKResult.Failure<VKAuthenticatedUser>(VKJwtErrors.ConfigurationError);
            }

            TokenValidationParameters validationParameters = JwtValidationFactory.Create(jwtOptions);

            // PERF: Using modern JsonWebTokenHandler for better performance and safety.
            TokenValidationResult validationResult = await _tokenHandler.ValidateTokenAsync(token, validationParameters).ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                VKError failureReason = validationResult.Exception switch
                {
                    SecurityTokenExpiredException => VKJwtErrors.Expired,
                    _ => VKJwtErrors.Invalid
                };

                logger.LogAuthenticationFailed(validationResult.Exception, failureReason.Code);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, failureReason.Code);
                return VKResult.Failure<VKAuthenticatedUser>(failureReason);
            }

            var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);

            // 1. Perform Revocation Check (using unified method with caching)
            VKResult revocationResult = await ValidateRevocationAsync(principal, cancellationToken).ConfigureAwait(false);
            if (revocationResult.IsFailure)
            {
                return VKResult.Failure<VKAuthenticatedUser>(revocationResult.FirstError);
            }

            // 2. Map to result and perform final integrity checks via extension method
            VKResult<VKAuthenticatedUser> mappingResult = principal.ToAuthenticatedUser();
            if (mappingResult.IsFailure)
            {
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, mappingResult.FirstError.Code);
                return VKResult.Failure<VKAuthenticatedUser>(mappingResult.FirstError);
            }

            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, true);
            activity?.SetTag(AuthenticationDiagnosticsConstants.TagUserId, principal.GetUserId());
            return VKResult.Success(mappingResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedAuthenticationError(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, VKJwtErrors.Invalid.Code);
            return VKResult.Failure<VKAuthenticatedUser>(VKJwtErrors.Invalid);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> ValidateRevocationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        // 1. User-level revocation check
        string? userId = principal.GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            // Request-scoped cache check to avoid redundant DB/Auth-Service hits
            if (_revocationCache.TryGetValue(userId, out bool isRevoked))
            {
                return isRevoked ? VKResult.Failure(VKJwtErrors.Revoked) : VKResult.Success();
            }

            if (await revocationProvider.IsUserRevokedAsync(userId, cancellationToken).ConfigureAwait(false))
            {
                _revocationCache[userId] = true;
                AuthenticationDiagnostics.RecordRevocationHit(AuthenticationDiagnosticsConstants.TypeJwt);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, VKJwtErrors.Revoked.Code);
                return VKResult.Failure(VKJwtErrors.Revoked);
            }

            _revocationCache[userId] = false;
        }

        // 2. Token-level (jti) revocation check
        string? jti = principal.GetJti();
        if (!string.IsNullOrEmpty(jti))
        {
            if (_revocationCache.TryGetValue(jti, out bool isRevoked))
            {
                return isRevoked ? VKResult.Failure(VKJwtErrors.Revoked) : VKResult.Success();
            }

            if (await revocationProvider.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false))
            {
                _revocationCache[jti] = true;
                AuthenticationDiagnostics.RecordRevocationHit(AuthenticationDiagnosticsConstants.TypeJwt);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, VKJwtErrors.Revoked.Code);
                return VKResult.Failure(VKJwtErrors.Revoked);
            }

            _revocationCache[jti] = false;
        }

        return VKResult.Success();
    }
}
