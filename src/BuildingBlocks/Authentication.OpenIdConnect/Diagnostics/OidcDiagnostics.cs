using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Authentication.OpenIdConnect.Contracts;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.Authentication.OpenIdConnect.Diagnostics;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication.OpenIdConnect building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics<OidcBlock>]
internal static partial class OidcDiagnostics
{
    private static readonly Counter<long> _authenticationRequests;

    private static readonly Histogram<double> _validationDuration;

    static OidcDiagnostics()
    {
        _authenticationRequests = Meter.CreateCounter<long>(
            OidcDiagnosticsConstants.AuthRequestCounterName,
            description: "Number of OIDC authentication attempts"
        );

        _validationDuration = Meter.CreateHistogram<double>(
            OidcDiagnosticsConstants.AuthDurationHistogramName,
            unit: "ms",
            description: "Duration of OIDC authentication/validation"
        );
    }

    /// <summary>
    /// Records an OIDC authentication attempt result.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="isSuccess">Whether the attempt was successful.</param>
    /// <param name="failureReason">The reason for failure if not successful.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    public static void RecordAuthAttempt(string provider, bool isSuccess, string? failureReason = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { OidcDiagnosticsConstants.TagAuthProvider, provider },
            { OidcDiagnosticsConstants.TagAuthResult, isSuccess ? OidcDiagnosticsConstants.ValueSuccess : OidcDiagnosticsConstants.ValueFailure }
        };

        if (!string.IsNullOrEmpty(tenantId))
        {
            tags.Add(OidcDiagnosticsConstants.TagTenantId, tenantId);
        }

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            tags.Add(OidcDiagnosticsConstants.TagAuthFailureReason, failureReason);
        }

        _authenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of an OIDC validation attempt.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="isSuccess">Whether the validation was successful.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    public static void RecordDuration(string provider, double durationMs, bool isSuccess, string? tenantId = null)
    {
        var tags = new TagList
        {
            { OidcDiagnosticsConstants.TagAuthProvider, provider },
            { OidcDiagnosticsConstants.TagAuthResult, isSuccess ? OidcDiagnosticsConstants.ValueSuccess : OidcDiagnosticsConstants.ValueFailure }
        };

        if (!string.IsNullOrEmpty(tenantId))
        {
            tags.Add(OidcDiagnosticsConstants.TagTenantId, tenantId);
        }

        _validationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Starts a new activity for OIDC token validation.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <returns>The started <see cref="Activity"/> or null if diagnostics are disabled.</returns>
    public static Activity? StartOidcValidation(string provider)
    {
        var activity = Source.StartActivity(OidcDiagnosticsConstants.ActivityAuthenticateOidc);
        activity?.SetTag(OidcDiagnosticsConstants.TagAuthProvider, provider);
        return activity;
    }
}



