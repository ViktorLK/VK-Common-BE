using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Diagnostics.Models;
using VK.Blocks.Authentication.Generated;
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

    private static readonly Counter<long> _authenticationRequests;

    private static readonly Counter<long> _tooManyRequests;

    private static readonly Counter<long> _revocations;

    private static readonly Counter<long> _replayAttacks;

    private static readonly Counter<long> _claimsTransformations;

    /// <summary>
    /// Histogram tracking the duration of claims transformation.
    /// </summary>
    private static readonly Histogram<double> _claimsTransformationDuration;

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

        _claimsTransformationDuration = Meter.CreateHistogram<double>(
            AuthenticationDiagnosticsConstants.ClaimsTransformationDurationName,
            unit: AuthenticationDiagnosticsConstants.ClaimsTransformationDurationUnit,
            description: AuthenticationDiagnosticsConstants.ClaimsTransformationDurationDescription
        );
    }

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
        _claimsTransformationDuration.Record(durationMs, tags);
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

    /// <summary>
    /// Gets the compile-time authentication metadata (topology).
    /// </summary>
    /// <returns>A map of endpoint names to authentication information.</returns>
    public static IReadOnlyDictionary<string, EndpointAuthenticationInfo> GetAuthenticationMetadata()
        => AuthenticationMetadata.Endpoints;

    /// <summary>
    /// Gets the deterministic hash of the authentication metadata.
    /// </summary>
    public static string GetMetadataHash() => AuthenticationMetadata.MetadataHash;

    /// <summary>
    /// Gets runtime information about all registered authentication schemes.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve scheme information.</param>
    /// <returns>A collection of registered authentication schemes.</returns>
    public static async Task<IEnumerable<AuthenticationSchemeInfo>> GetRegisteredSchemesAsync(IServiceProvider serviceProvider)
    {
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var schemes = await schemeProvider.GetAllSchemesAsync().ConfigureAwait(false);
        var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);

        return schemes.Select(s => new AuthenticationSchemeInfo
        {
            Name = s.Name,
            DisplayName = s.DisplayName,
            HandlerType = s.HandlerType.Name,
            IsDefault = defaultScheme?.Name == s.Name
        });
    }
}
