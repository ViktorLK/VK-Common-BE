using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.Web;

/// <summary>
/// Configuration options for the Identity.Web module.
/// Follows AP.01, AP.04, BB.05.
/// </summary>
[VKOptions]
public sealed partial record VKIdentityWebOptions : IVKBlockOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKIdentityWebOptions"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public VKIdentityWebOptions() { }

    /// <summary>
    /// Gets or sets the claim type used to extract the user's strongly-typed identifier.
    /// Default is <see cref="VKClaimConstants.UserId"/>.
    /// </summary>
    public string UserIdClaimType { get; init; } = VKClaimConstants.UserId;

    /// <summary>
    /// Gets or sets a value indicating whether unauthenticated requests are allowed to proceed with <see cref="VKUserId.Anonymous"/>.
    /// Default is <c>true</c>.
    /// </summary>
    public bool AllowAnonymous { get; init; } = true;
}
