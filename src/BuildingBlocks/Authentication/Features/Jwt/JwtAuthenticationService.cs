using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Features.Jwt.RefreshTokens;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Implements <see cref="IJwtAuthenticationService"/> to validate JWT tokens and map them to <see cref="AuthenticatedUser"/>.
/// Uses the modern <see cref="JsonWebTokenHandler"/> for high-performance validation.
/// </summary>
internal sealed class JwtAuthenticationService(
    IOptionsMonitor<JwtOptions> options,
    ILogger<JwtAuthenticationService> logger,
    IJwtTokenRevocationProvider revocationProvider) : IJwtAuthenticationService
{
    #region Fields

    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly Dictionary<string, bool> _revocationCache = [];

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<Result<AuthenticatedUser>> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        using var activity = AuthenticationDiagnostics.StartJwtValidation();

        if (string.IsNullOrWhiteSpace(token))
        {
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, JwtErrors.EmptyToken.Code);
            return Result.Failure<AuthenticatedUser>(JwtErrors.EmptyToken);
        }

        try
        {
            var jwtOptions = options.CurrentValue;

            if (string.IsNullOrEmpty(jwtOptions.SecretKey))
            {
                logger.LogOptionsNotConfigured();
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, JwtErrors.ConfigurationError.Code);
                return Result.Failure<AuthenticatedUser>(JwtErrors.ConfigurationError);
            }

            var validationParameters = JwtValidationFactory.Create(jwtOptions);

            // PERF: Using modern JsonWebTokenHandler for better performance and safety.
            var validationResult = await _tokenHandler.ValidateTokenAsync(token, validationParameters).ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                var failureReason = validationResult.Exception switch
                {
                    SecurityTokenExpiredException => JwtErrors.Expired,
                    _ => JwtErrors.Invalid
                };

                logger.LogAuthenticationFailed(validationResult.Exception, failureReason.Code);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, failureReason.Code);
                return Result.Failure<AuthenticatedUser>(failureReason);
            }

            var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);

            // 1. Perform Revocation Check (using unified method with caching)
            var revocationResult = await ValidateRevocationAsync(principal, cancellationToken).ConfigureAwait(false);
            if (revocationResult.IsFailure)
            {
                return Result.Failure<AuthenticatedUser>(revocationResult.FirstError);
            }

            // 2. Map to result and perform final integrity checks via extension method
            var mappingResult = principal.ToAuthenticatedUser();
            if (mappingResult.IsFailure)
            {
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, mappingResult.FirstError.Code);
                return Result.Failure<AuthenticatedUser>(mappingResult.FirstError);
            }

            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, true);
            activity?.SetTag(AuthenticationDiagnosticsConstants.TagUserId, principal.GetUserId());
            return Result.Success(mappingResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedAuthenticationError(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, JwtErrors.Invalid.Code);
            return Result.Failure<AuthenticatedUser>(JwtErrors.Invalid);
        }
    }

    /// <inheritdoc />
    public async Task<Result> ValidateRevocationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        // 1. User-level revocation check
        var userId = principal.GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            // Request-scoped cache check to avoid redundant DB/Auth-Service hits
            if (_revocationCache.TryGetValue(userId, out var isRevoked))
            {
                return isRevoked ? Result.Failure(JwtErrors.Revoked) : Result.Success();
            }

            if (await revocationProvider.IsUserRevokedAsync(userId, cancellationToken).ConfigureAwait(false))
            {
                _revocationCache[userId] = true;
                AuthenticationDiagnostics.RecordRevocationHit(AuthenticationDiagnosticsConstants.TypeJwt);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, JwtErrors.Revoked.Code);
                return Result.Failure(JwtErrors.Revoked);
            }

            _revocationCache[userId] = false;
        }

        // 2. Token-level (jti) revocation check
        var jti = principal.GetJti();
        if (!string.IsNullOrEmpty(jti))
        {
            if (_revocationCache.TryGetValue(jti, out var isRevoked))
            {
                return isRevoked ? Result.Failure(JwtErrors.Revoked) : Result.Success();
            }

            if (await revocationProvider.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false))
            {
                _revocationCache[jti] = true;
                AuthenticationDiagnostics.RecordRevocationHit(AuthenticationDiagnosticsConstants.TypeJwt);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeJwt, false, JwtErrors.Revoked.Code);
                return Result.Failure(JwtErrors.Revoked);
            }

            _revocationCache[jti] = false;
        }

        return Result.Success();
    }

    #endregion
}
