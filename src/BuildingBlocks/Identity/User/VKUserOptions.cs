using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Configuration options for User feature slice.
/// Follows [BB.05].
/// </summary>
public sealed partial record VKUserOptions : IVKBlockOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKUserOptions"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public VKUserOptions() { }

    /// <summary>
    /// Gets or sets a value indicating whether newly created users require email verification. Default is false.
    /// </summary>
    public bool RequireEmailVerification { get; init; } = false;
}
