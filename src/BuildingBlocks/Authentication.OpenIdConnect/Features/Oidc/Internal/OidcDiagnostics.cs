using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication.OpenIdConnect building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics(OidcDiagnosticsConstants.SourceName)]
internal static partial class OidcDiagnostics
{
    #region Fields

    /// <summary>
    /// Counter tracking the number of OIDC authentication attempts.
    /// </summary>
    public static readonly Counter<long> AuthenticationRequests;

    /// <summary>
    /// Histogram tracking the duration of OIDC authentication/validation.
    /// </summary>
    public static readonly Histogram<double> ValidationDuration;

    #endregion

    #region Constructors

    static OidcDiagnostics()
    {
        AuthenticationRequests = Meter.CreateCounter<long>(
            OidcDiagnosticsConstants.AuthRequestCounterName,
            description: "Number of OIDC authentication attempts"
        );

        ValidationDuration = Meter.CreateHistogram<double>(
            OidcDiagnosticsConstants.AuthDurationHistogramName,
            unit: "ms",
            description: "Duration of OIDC authentication/validation"
        );
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Records an OIDC authentication attempt result.
    /// </summary>
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

        AuthenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of an OIDC validation attempt.
    /// </summary>
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

        ValidationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Starts a new activity for OIDC token validation.
    /// </summary>
    public static Activity? StartOidcValidation(string provider)
    {
        var activity = Source.StartActivity(OidcDiagnosticsConstants.ActivityAuthenticateOidc);
        activity?.SetTag(OidcDiagnosticsConstants.TagAuthProvider, provider);
        return activity;
    }

    #endregion
}
