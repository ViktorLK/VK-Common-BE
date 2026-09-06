using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Top-level configuration options for the Identity Building Block.
/// Follows [BB.05].
/// </summary>
public sealed partial record VKIdentityOptions : IVKBlockOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKIdentityOptions"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public VKIdentityOptions() { }

    /// <summary>
    /// Gets or sets a value indicating whether multiple tenants are allowed. Default is true.
    /// </summary>
    public bool MultiTenancyEnabled { get; init; } = true;
}
