using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Configuration options for Tenant feature slice.
/// Follows [BB.05].
/// </summary>
public sealed partial record VKTenantOptions : IVKBlockOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKTenantOptions"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public VKTenantOptions() { }

    /// <summary>
    /// Gets or sets a value indicating whether new tenants default to trial status. Default is false.
    /// </summary>
    public bool DefaultToTrial { get; init; } = false;

    /// <summary>
    /// Gets or sets the default trial period in days. Default is 14.
    /// </summary>
    public int TrialPeriodDays { get; init; } = 14;
}
