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

    private static readonly Counter<long> _authenticationRequests;

    private static readonly Counter<long> _tooManyRequests;

    private static readonly Counter<long> _revocations;

    private static readonly Counter<long> _replayAttacks;

    private static readonly Counter<long> _claimsTransformations;

    /// <summary>
    /// Histogram tracking the duration of claims transformation.
    /// </summary>
    private static readonly Histogram<double> ClaimsTransformationDuration;

    #endregion

    #region Constructors

    static AuthenticationDiagnostics()
    {
        _authenticationRequests = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.AuthRequestCounterName,
            description: AuthenticationDiagnosticsConstants.AuthRequestCounterDescription
        );

        _tooManyRequests = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.TooManyRequestsCounterName,
            unit: AuthenticationDiagnosticsConstants.TooManyRequestsCounterUnit,
            description: AuthenticationDiagnosticsConstants.TooManyRequestsCounterDescription
        );

        _revocations = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.RevocationCounterName,
            description: AuthenticationDiagnosticsConstants.RevocationCounterDescription
        );

        _replayAttacks = Meter.CreateCounter<long>(
            AuthenticationDiagnosticsConstants.ReplayCounterName,
            description: AuthenticationDiagnosticsConstants.ReplayCounterDescription
        );

        _claimsTransformations = Meter.CreateCounter<long>(
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

        _authenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records an authentication rejection due to revocation.
    /// </summary>
    /// <param name="authType">The type of authentication.</param>
    public static void RecordRevocationHit(string authType)
    {
        _revocations.Add(1, new KeyValuePair<string, object?>(AuthenticationDiagnosticsConstants.TagAuthType, authType));
    }

    /// <summary>
    /// Records a detected refresh token replay attack.
    /// </summary>
    /// <param name="familyId">The family identifier of the token.</param>
    public static void RecordReplayAttack(string familyId)
    {
        _replayAttacks.Add(1, new KeyValuePair<string, object?>(AuthenticationDiagnosticsConstants.TagUserId, familyId));
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

        _claimsTransformations.Add(1, tags);
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

        _tooManyRequests.Add(1, tags);
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
