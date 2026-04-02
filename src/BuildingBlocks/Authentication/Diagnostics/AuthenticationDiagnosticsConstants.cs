namespace VK.Blocks.Authentication.Diagnostics;

/// <summary>
/// Internal constants for the Authentication diagnostics feature.
/// </summary>
internal static class AuthenticationDiagnosticsConstants
{
    #region Fields

    #region Source and Meter

    /// <summary>
    /// The diagnostic source name for the Authentication block.
    /// </summary>
    public const string SourceName = "VK.Blocks.Authentication";

    #endregion

    #region Counters

    /// <summary>
    /// Counter name for tracking authentication requests.
    /// </summary>
    public const string AuthRequestCounterName = "authentication.requests";

    /// <summary>
    /// Description for the authentication requests counter.
    /// </summary>
    public const string AuthRequestCounterDescription = "Number of authentication requests processed";

    /// <summary>
    /// Counter name for tracking rate limit violations.
    /// </summary>
    public const string TooManyRequestsCounterName = "vk.auth.too_many_requests";

    /// <summary>
    /// Unit of measurement for rate limit violations.
    /// </summary>
    public const string TooManyRequestsCounterUnit = "requests";

    /// <summary>
    /// Description for the rate limit violations counter.
    /// </summary>
    public const string TooManyRequestsCounterDescription = "The number of requests that exceeded the rate limit.";

    /// <summary>
    /// Counter name for tracking revocation hits.
    /// </summary>
    public const string RevocationCounterName = "vk.auth.revocations";

    /// <summary>
    /// Description for the revocation hits counter.
    /// </summary>
    public const string RevocationCounterDescription = "Number of requests rejected due to token/user revocation";

    /// <summary>
    /// Counter name for tracking refresh token replay attacks.
    /// </summary>
    public const string ReplayCounterName = "vk.auth.replay_detection";

    /// <summary>
    /// Description for the replay detection counter.
    /// </summary>
    public const string ReplayCounterDescription = "Number of detected refresh token replay attacks";

    /// <summary>
    /// Counter name for tracking claims transformation attempts.
    /// </summary>
    public const string ClaimsTransformationCounterName = "vk.auth.claims_transformation.count";

    /// <summary>
    /// Description for the claims transformation counter.
    /// </summary>
    public const string ClaimsTransformationCounterDescription = "Number of claims transformation attempts";

    /// <summary>
    /// Histogram name for tracking claims transformation duration.
    /// </summary>
    public const string ClaimsTransformationDurationName = "vk.auth.claims_transformation.duration";

    /// <summary>
    /// Unit of measurement for claims transformation duration.
    /// </summary>
    public const string ClaimsTransformationDurationUnit = "ms";

    /// <summary>
    /// Description for the claims transformation duration histogram.
    /// </summary>
    public const string ClaimsTransformationDurationDescription = "Time taken for claims transformation";

    #endregion

    #region Activity Names

    /// <summary>
    /// Activity name for JWT authentication.
    /// </summary>
    public const string ActivityAuthenticateJwt = "JwtAuthenticationService.AuthenticateAsync";

    /// <summary>
    /// Activity name for API key validation.
    /// </summary>
    public const string ActivityValidateApiKey = "ApiKeyValidator.ValidateAsync";

    /// <summary>
    /// Activity name for claims transformation.
    /// </summary>
    public const string ActivityTransformClaims = "VKClaimsTransformer.TransformAsync";

    /// <summary>
    /// Activity name for revocation checking.
    /// </summary>
    public const string ActivityIsRevoked = "JwtTokenRevocationService.IsRevokedAsync";

    #endregion

    #region Tag Keys

    /// <summary>
    /// Tag key for the authentication type.
    /// </summary>
    public const string TagAuthType = "auth.type";

    /// <summary>
    /// Tag key for the authentication result.
    /// </summary>
    public const string TagAuthResult = "auth.result";

    /// <summary>
    /// Tag key for the user identifier.
    /// </summary>
    public const string TagUserId = "auth.user.id";

    /// <summary>
    /// Tag key for the API key identifier.
    /// </summary>
    public const string TagKeyId = "auth.key_id";

    /// <summary>
    /// Tag key for the tenant identifier.
    /// </summary>
    public const string TagTenantId = "auth.tenant_id";

    /// <summary>
    /// Tag key for the failure reason.
    /// </summary>
    public const string TagFailureReason = "auth.failure_reason";

    /// <summary>
    /// Tag key for indicating if claims were transformed.
    /// </summary>
    public const string TagClaimsTransformed = "auth.claims_transformed";

    #endregion

    #region Tag Values

    /// <summary>
    /// Tag value for a successful operation.
    /// </summary>
    public const string ValueSuccess = "Success";

    /// <summary>
    /// Tag value for a failed operation.
    /// </summary>
    public const string ValueFailure = "Failure";

    /// <summary>
    /// Tag value for JWT authentication type.
    /// </summary>
    public const string TypeJwt = "jwt";

    /// <summary>
    /// Tag value for API key authentication type.
    /// </summary>
    public const string TypeApiKey = "apikey";

    #endregion

    #endregion
}
