using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Diagnostics;

/// <summary>
/// Provides centralized diagnostics and telemetry for the Authorization block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics(AuthorizationDiagnosticsConstants.SourceName)]
internal static partial class AuthorizationDiagnostics
{
    // ActivitySource and Meter are generated automatically into a partial class.

    #region Fields

    private static readonly Counter<long> _authorizationDecisions;

    private static readonly Counter<long> _failureReasons;

    private static readonly Histogram<double> _evaluationDuration;

    #endregion

    #region Constructors

    static AuthorizationDiagnostics()
    {
        _authorizationDecisions = Meter.CreateCounter<long>(
            AuthorizationDiagnosticsConstants.DecisionCounterName,
            description: AuthorizationDiagnosticsConstants.DecisionCounterDescription);

        _failureReasons = Meter.CreateCounter<long>(
            AuthorizationDiagnosticsConstants.FailureReasonsCounterName,
            description: AuthorizationDiagnosticsConstants.FailureReasonsCounterDescription);

        _evaluationDuration = Meter.CreateHistogram<double>(
            AuthorizationDiagnosticsConstants.EvaluationDurationName,
            unit: AuthorizationDiagnosticsConstants.EvaluationDurationUnit,
            description: AuthorizationDiagnosticsConstants.EvaluationDurationDescription);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Records an authorization decision.
    /// </summary>
    /// <param name="policyName">The name of the policy being evaluated.</param>
    /// <param name="isAllowed">Whether the authorization was successful.</param>
    internal static void RecordDecision(string policyName, bool isAllowed)
    {
        _authorizationDecisions.Add(1,
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }

    /// <summary>
    /// Records the specific error code for an authorization failure.
    /// </summary>
    /// <param name="policyName">The name of the policy that failed.</param>
    /// <param name="error">The <see cref="Error"/> representing the failure.</param>
    internal static void RecordFailure(string policyName, Error error)
    {
        _failureReasons.Add(1,
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagErrorCode, error.Code));
    }

    /// <summary>
    /// Records the duration of an authorization evaluation.
    /// </summary>
    /// <param name="policyName">The name of the policy being evaluated.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="isAllowed">Whether the authorization was successful.</param>
    internal static void RecordEvaluationDuration(string policyName, double durationMs, bool isAllowed)
    {
        _evaluationDuration.Record(durationMs,
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }

    #endregion
}
