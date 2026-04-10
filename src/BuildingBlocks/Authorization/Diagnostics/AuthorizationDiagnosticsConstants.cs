namespace VK.Blocks.Authorization.Diagnostics;

/// <summary>
/// Internal constants for the Authorization diagnostics feature.
/// </summary>
internal static class AuthorizationDiagnosticsConstants
{
    #region Fields

    #region Source and Meter

    /// <summary>
    /// The diagnostic source name for the Authorization block.
    /// </summary>
    public const string SourceName = "VK.Blocks.Authorization";

    #endregion

    #region Counters and Histograms

    /// <summary>
    /// Counter name for tracking authorization decisions.
    /// </summary>
    public const string DecisionCounterName = "authorization.decisions";

    /// <summary>
    /// Description for the authorization decisions counter.
    /// </summary>
    public const string DecisionCounterDescription = "Number of authorization decisions made";

    /// <summary>
    /// Counter name for tracking authorization failure reasons.
    /// </summary>
    public const string FailureReasonsCounterName = "authorization.failure.reasons";

    /// <summary>
    /// Description for the authorization failure reasons counter.
    /// </summary>
    public const string FailureReasonsCounterDescription = "Count of authorization failures by error code";

    /// <summary>
    /// Histogram name for tracking authorization evaluation duration.
    /// </summary>
    public const string EvaluationDurationName = "authorization.evaluation.duration";

    /// <summary>
    /// Unit of measurement for authorization evaluation duration.
    /// </summary>
    public const string EvaluationDurationUnit = "ms";

    /// <summary>
    /// Description for the authorization evaluation duration histogram.
    /// </summary>
    public const string EvaluationDurationDescription = "Time taken to evaluate authorization requirements";

    #endregion

    #region Tag Keys

    /// <summary>
    /// Tag key for the authorization policy name.
    /// </summary>
    public const string TagPolicyName = "authorization.policy";

    /// <summary>
    /// Tag key for the authorization decision (Allowed/Denied).
    /// </summary>
    public const string TagDecision = "authorization.decision";

    /// <summary>
    /// Tag key for the error code in case of failure.
    /// </summary>
    public const string TagErrorCode = "authorization.error_code";

    #endregion

    #endregion
}
