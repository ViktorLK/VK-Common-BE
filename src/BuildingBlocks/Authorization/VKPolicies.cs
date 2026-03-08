namespace VK.Blocks.Authorization;

/// <summary>
/// Well-known VK authorization policy names used by the builder extensions.
/// </summary>
public static class VKPolicies
{
    #region Fields

    /// <summary>
    /// Policy indicating access is restricted to working hours only.
    /// </summary>
    public const string WorkingHoursOnly = nameof(WorkingHoursOnly);

    /// <summary>
    /// Policy indicating access is restricted to internal network origins only.
    /// </summary>
    public const string InternalNetworkOnly = nameof(InternalNetworkOnly);

    /// <summary>
    /// Policy indicating access requires senior or higher employee rank.
    /// </summary>
    public const string SeniorAndAbove = nameof(SeniorAndAbove);

    /// <summary>
    /// Policy indicating access requires financial data write permissions.
    /// </summary>
    public const string FinancialDataWrite = nameof(FinancialDataWrite);

    #endregion
}

