using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Authentication.Diagnostics;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics(AuthenticationDiagnosticsConstants.SourceName)]
public static partial class AuthenticationDiagnostics
{
    // ActivitySource and Meter are generated automatically into a partial class.

    #region Fields

    /// <summary>
    /// Counter tracking the number of authentication attempts.
    /// Includes tags for "auth.type" (e.g., JWT, ApiKey) and "auth.result" (Success, Failure).
    /// </summary>
    public static readonly Counter<long> AuthenticationRequests;

    /// <summary>
    /// Counter tracking the number of API key rate limit violations.
    /// </summary>
    public static readonly Counter<long> TooManyRequests;

    /// <summary>
    /// Counter tracking the number of authentication requests rejected due to revocation.
    /// </summary>
    public static readonly Counter<long> Revocations;

    /// <summary>
    /// Counter tracking the number of detected refresh token replay attacks.
    /// </summary>
    public static readonly Counter<long> ReplayAttacks;

    /// <summary>
    /// Counter tracking the number of claims transformation attempts.
    /// </summary>
    public static readonly Counter<long> ClaimsTransformations;

    /// <summary>
    /// Histogram tracking the duration of claims transformation.
    /// </summary>
    public static readonly Histogram<double> ClaimsTransformationDuration;

    #endregion

    #region Constructors

    static AuthenticationDiagnostics()
    {
        AuthenticationRequests = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.AuthRequestCounterName,
            description: AuthenticationDiagnosticsConstants.AuthRequestCounterDescription
        );

        TooManyRequests = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.TooManyRequestsCounterName,
            unit: AuthenticationDiagnosticsConstants.TooManyRequestsCounterUnit,
            description: AuthenticationDiagnosticsConstants.TooManyRequestsCounterDescription
        );

        Revocations = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.RevocationCounterName,
            description: AuthenticationDiagnosticsConstants.RevocationCounterDescription
        );

        ReplayAttacks = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.ReplayCounterName,
            description: AuthenticationDiagnosticsConstants.ReplayCounterDescription
        );

        ClaimsTransformations = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.ClaimsTransformationCounterName,
            description: AuthenticationDiagnosticsConstants.ClaimsTransformationCounterDescription
        );

        ClaimsTransformationDuration = Meter.CreateHistogram<double>(
            AuthenticationDiagnosticsConstants.ClaimsTransformationDurationName,
            unit: AuthenticationDiagnosticsConstants.ClaimsTransformationDurationUnit,
            description: AuthenticationDiagnosticsConstants.ClaimsTransformationDurationDescription
        );
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Records an authentication attempt result.
    /// </summary>
    /// <param name="authType">The type of authentication (e.g., "jwt", "apikey").</param>
    /// <param name="isSuccess">Whether the authentication succeeded.</param>
    /// <param name="failureReason">The reason for failure, if any.</param>
    public static void RecordAuthAttempt(string authType, bool isSuccess, string? failureReason = null)
    {
        var tags = new TagList
        {
            { AuthenticationDiagnosticsConstants.TagAuthType, authType },
            { AuthenticationDiagnosticsConstants.TagAuthResult, isSuccess ? AuthenticationDiagnosticsConstants.ValueSuccess : AuthenticationDiagnosticsConstants.ValueFailure }
        };

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            tags.Add(AuthenticationDiagnosticsConstants.TagFailureReason, failureReason);
        }

        AuthenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records an authentication rejection due to revocation.
    /// </summary>
    /// <param name="authType">The type of authentication.</param>
    public static void RecordRevocationHit(string authType)
    {
        Revocations.Add(1, new KeyValuePair<string, object?>(AuthenticationDiagnosticsConstants.TagAuthType, authType));
    }

    /// <summary>
    /// Records a detected refresh token replay attack.
    /// </summary>
    /// <param name="familyId">The family identifier of the token.</param>
    public static void RecordReplayAttack(string familyId)
    {
        ReplayAttacks.Add(1, new KeyValuePair<string, object?>(AuthenticationDiagnosticsConstants.TagUserId, familyId));
    }

    /// <summary>
    /// Records a claims transformation attempt and its duration.
    /// </summary>
    /// <param name="durationMs">The duration of the transformation in milliseconds.</param>
    /// <param name="applied">Whether transformation was actually applied.</param>
    public static void RecordClaimsTransformation(double durationMs, bool applied)
    {
        var tags = new TagList
        {
            { AuthenticationDiagnosticsConstants.TagClaimsTransformed, applied }
        };

        ClaimsTransformations.Add(1, tags);
        ClaimsTransformationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records a rate limit exceeded event with specific key and tenant context.
    /// </summary>
    /// <param name="keyId">The identifier of the API key.</param>
    /// <param name="tenantId">The identifier of the tenant (optional).</param>
    public static void RecordTooManyRequests(string keyId, string? tenantId)
    {
        var tags = new TagList
        {
            { AuthenticationDiagnosticsConstants.TagKeyId, keyId },
            { AuthenticationDiagnosticsConstants.TagTenantId, tenantId ?? string.Empty }
        };

        TooManyRequests.Add(1, tags);
    }

    #endregion

    #region Activity Factories

    /// <summary>
    /// Starts a new activity for JWT validation.
    /// </summary>
    public static Activity? StartJwtValidation()
        => Source.StartActivity(AuthenticationDiagnosticsConstants.ActivityAuthenticateJwt);

    /// <summary>
    /// Starts a new activity for API key validation.
    /// </summary>
    public static Activity? StartApiKeyValidation()
        => Source.StartActivity(AuthenticationDiagnosticsConstants.ActivityValidateApiKey);

    /// <summary>
    /// Starts a new activity for claims transformation.
    /// </summary>
    public static Activity? StartClaimsTransformation()
        => Source.StartActivity(AuthenticationDiagnosticsConstants.ActivityTransformClaims);

    #endregion
}
