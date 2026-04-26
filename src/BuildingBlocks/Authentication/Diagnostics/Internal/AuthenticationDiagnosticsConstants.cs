using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Authentication.Diagnostics.Internal;

/// <summary>
/// Internal constants for the Authentication diagnostics feature.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static constants for diagnostics and telemetry tagging.")]
internal static class AuthenticationDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Authentication block.
    /// </summary>
    internal static readonly string SourceName = VKAuthenticationBlock.Instance.ActivitySourceName;

    /// <summary>
    /// Counter name for tracking authentication requests.
    /// </summary>
    internal const string AuthRequestCounterName = "authentication.requests";

    /// <summary>
    /// Description for the authentication requests counter.
    /// </summary>
    internal const string AuthRequestCounterDescription = "Number of authentication requests processed";

    /// <summary>
    /// Counter name for tracking rate limit violations.
    /// </summary>
    internal const string TooManyRequestsCounterName = "vk.auth.too_many_requests";

    /// <summary>
    /// Unit of measurement for rate limit violations.
    /// </summary>
    internal const string TooManyRequestsCounterUnit = "requests";

    /// <summary>
    /// Description for the rate limit violations counter.
    /// </summary>
    internal const string TooManyRequestsCounterDescription = "The number of requests that exceeded the rate limit.";

    /// <summary>
    /// Counter name for tracking revocation hits.
    /// </summary>
    internal const string RevocationCounterName = "vk.auth.revocations";

    /// <summary>
    /// Description for the revocation hits counter.
    /// </summary>
    internal const string RevocationCounterDescription = "Number of requests rejected due to token/user revocation";

    /// <summary>
    /// Counter name for tracking refresh token replay attacks.
    /// </summary>
    internal const string ReplayCounterName = "vk.auth.replay_detection";

    /// <summary>
    /// Description for the replay detection counter.
    /// </summary>
    internal const string ReplayCounterDescription = "Number of detected refresh token replay attacks";

    /// <summary>
    /// Counter name for tracking claims transformation attempts.
    /// </summary>
    internal const string ClaimsTransformationCounterName = "vk.auth.claims_transformation.count";

    /// <summary>
    /// Description for the claims transformation counter.
    /// </summary>
    internal const string ClaimsTransformationCounterDescription = "Number of claims transformation attempts";

    /// <summary>
    /// Histogram name for tracking claims transformation duration.
    /// </summary>
    internal const string ClaimsTransformationDurationName = "vk.auth.claims_transformation.duration";

    /// <summary>
    /// Unit of measurement for claims transformation duration.
    /// </summary>
    internal const string ClaimsTransformationDurationUnit = "ms";

    /// <summary>
    /// Description for the claims transformation duration histogram.
    /// </summary>
    internal const string ClaimsTransformationDurationDescription = "Time taken for claims transformation";

    /// <summary>
    /// Activity name for JWT authentication.
    /// </summary>
    internal const string ActivityAuthenticateJwt = "JwtAuthenticationService.AuthenticateAsync";

    /// <summary>
    /// Activity name for API key validation.
    /// </summary>
    internal const string ActivityValidateApiKey = "ApiKeyValidator.ValidateAsync";

    /// <summary>
    /// Activity name for claims transformation.
    /// </summary>
    internal const string ActivityTransformClaims = "VKClaimsTransformer.TransformAsync";

    /// <summary>
    /// Activity name for revocation checking.
    /// </summary>
    internal const string ActivityIsRevoked = "JwtTokenRevocationService.IsRevokedAsync";

    /// <summary>
    /// Tag key for the authentication type.
    /// </summary>
    internal const string TagAuthType = "auth.type";

    /// <summary>
    /// Tag key for the authentication result.
    /// </summary>
    internal const string TagAuthResult = "auth.result";

    /// <summary>
    /// Tag key for the user identifier.
    /// </summary>
    internal const string TagUserId = "auth.user.id";

    /// <summary>
    /// Tag key for the API key identifier.
    /// </summary>
    internal const string TagKeyId = "auth.key_id";

    /// <summary>
    /// Tag key for the tenant identifier.
    /// </summary>
    internal const string TagTenantId = "auth.tenant_id";

    /// <summary>
    /// Tag key for the failure reason.
    /// </summary>
    internal const string TagFailureReason = "auth.failure_reason";

    /// <summary>
    /// Tag key for indicating if claims were transformed.
    /// </summary>
    internal const string TagClaimsTransformed = "auth.claims_transformed";

    /// <summary>
    /// Tag value for a successful operation.
    /// </summary>
    internal const string ValueSuccess = "Success";

    /// <summary>
    /// Tag value for a failed operation.
    /// </summary>
    internal const string ValueFailure = "Failure";

    /// <summary>
    /// Tag value for JWT authentication type.
    /// </summary>
    internal const string TypeJwt = "jwt";

    /// <summary>
    /// Tag value for API key authentication type.
    /// </summary>
    internal const string TypeApiKey = "apikey";
}
