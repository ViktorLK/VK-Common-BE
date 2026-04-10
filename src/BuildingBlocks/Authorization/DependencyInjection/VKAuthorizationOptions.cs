using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.InternalNetwork.Internal;
using VK.Blocks.Authorization.Features.WorkingHours;
using VK.Blocks.Authorization.Features.WorkingHours.Internal;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Root configuration options for the Authorization building block.
/// </summary>
public sealed class VKAuthorizationOptions
{
    #region Fields

    /// <summary>
    /// The configuration section name for authorization options.
    /// </summary>
    public const string SectionName = "Authorization";

    /// <summary>
    /// Gets or sets which built-in policies should be registered.
    /// Defaults to All.
    /// </summary>
    public VKAuthorizationPolicyFlags EnabledPolicies { get; set; } = VKAuthorizationPolicyFlags.All;

    #endregion

    #region Basic Control

    /// <summary>
    /// Gets or sets a value indicating whether authorization is enabled globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    #endregion

    #region Claim Types Configuration

    /// <summary>
    /// Gets or sets the claim type used to extract the user's role.
    /// Defaults to standard .NET Role claim.
    /// </summary>
    public string RoleClaimType { get; set; } = ClaimTypes.Role;

    /// <summary>
    /// Gets or sets the claim type used to extract user permissions.
    /// </summary>
    public string PermissionClaimType { get; set; } = VKAuthorizationClaimTypes.Permissions;

    /// <summary>
    /// Gets or sets the claim type used to extract the tenant identifier.
    /// </summary>
    public string TenantClaimType { get; set; } = VKAuthorizationClaimTypes.TenantId;

    /// <summary>
    /// Gets or sets the claim type used to extract the employee rank.
    /// </summary>
    public string RankClaimType { get; set; } = VKAuthorizationClaimTypes.Rank;

    #endregion

    #region Security Policies

    /// <summary>
    /// Gets or sets the role name that can bypass all authorization checks (SuperAdmin).
    /// If null or empty, bypass is disabled.
    /// </summary>
    public string SuperAdminRole { get; set; } = "SuperAdmin";

    /// <summary>
    /// Gets or sets a value indicating whether tenant isolation is strictly enforced.
    /// If false, users with the SuperAdmin role can view all tenants.
    /// </summary>
    public bool StrictTenantIsolation { get; set; } = true;

    #endregion

    #region Feature Defaults

    /// <summary>
    /// Gets or sets the start of the working hours window (local time).
    /// </summary>
    [Required]
    public TimeOnly WorkStart { get; set; } = WorkingHoursConstants.DefaultStart;

    /// <summary>
    /// Gets or sets the end of the working hours window (local time).
    /// </summary>
    [Required]
    public TimeOnly WorkEnd { get; set; } = WorkingHoursConstants.DefaultEnd;

    /// <summary>
    /// Gets or sets the list of allowed CIDR ranges for internal network policies.
    /// If empty, standard RFC 1918 private ranges are used.
    /// </summary>
    public IReadOnlyList<string> InternalCidrs { get; set; } = [.. InternalNetworkConstants.DefaultPrivateCidrs];

    #endregion
}
