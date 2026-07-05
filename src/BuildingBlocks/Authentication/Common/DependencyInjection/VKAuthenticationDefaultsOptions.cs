using Microsoft.AspNetCore.Authentication.JwtBearer;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Default configuration settings for the Authentication building block.
/// These values serve as fallbacks for all Authentication features.
/// Following BB.06: Modular Feature Pattern.
/// </summary>
[VKDefaults(typeof(VKAuthenticationBlock))]
public sealed partial record VKAuthenticationDefaultsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets the default authentication scheme.
    /// </summary>
    public string DefaultScheme { get; init; } = JwtBearerDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the interval for cleaning up the in-memory token cache.
    /// </summary>
    public int InMemoryCleanupIntervalMinutes { get; init; } = 15;
}
