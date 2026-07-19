using System;

namespace VK.Blocks.Authentication;

/// <summary>
/// Specifies the provider name for an <see cref="IOAuthClaimsMapper"/> implementation.
/// Used for dynamic registration during application startup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VKOAuthProviderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKOAuthProviderAttribute"/> class.
    /// </summary>
    /// <param name="providerName">The name of the OAuth provider (e.g., "Google", "GitHub").</param>
    public VKOAuthProviderAttribute(string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the OAuth provider.
    /// </summary>
    public string ProviderName { get; }
}
