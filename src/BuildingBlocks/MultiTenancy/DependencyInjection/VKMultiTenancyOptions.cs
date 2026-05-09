using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.Security.Internal;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Top-level configuration options for the multi-tenancy module.
/// </summary>
public sealed record VKMultiTenancyOptions : IVKBlockOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKMultiTenancyOptions"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public VKMultiTenancyOptions() { }

    /// <summary>
    /// The default configuration section name for multi-tenancy options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKMultiTenancyBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the multi-tenancy module is enabled.
    /// Default is <c>false</c>.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether tenant resolution is mandatory.
    /// When set to <c>true</c>, requests without a resolved tenant will receive a 401 response.
    /// Default is <c>true</c>.
    /// </summary>
    public bool RequireTenant { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether administrative impersonation is enabled.
    /// If true, users with specific headers can override the resolved tenant ID.
    /// MUST be combined with security middleware for validation.
    /// </summary>
    public bool EnableImpersonation { get; init; } = false;

    /// <summary>
    /// Gets or sets the role required for tenant impersonation.
    /// Default is "SuperAdmin".
    /// </summary>
    public string SuperAdminRole { get; init; } = MultiTenancySecurityConstants.SuperAdminRole;

    /// <summary>
    /// Gets or sets the list of enabled resolver types.
    /// When empty, all registered resolvers will be used.
    /// </summary>
    public IReadOnlyList<VKTenantResolverType> EnabledResolvers { get; set; } = [];
}
