namespace VK.Blocks.Generators.Authentication.Internal;

/// <summary>
/// Intermediate model for provider information used during the discovery process.
/// </summary>
/// <param name="FullName">The full type name of the provider class.</param>
internal sealed record ProviderInfo(string FullName);
