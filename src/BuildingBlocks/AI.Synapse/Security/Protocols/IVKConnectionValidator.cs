using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Security guard service responsible for validating connection overrides (SSRF protection & BYOK ownership).
/// </summary>
public interface IVKConnectionValidator
{
    /// <summary>
    /// Validates whether the given endpoint URI is in the permitted allowlist.
    /// Prevents SSRF attacks from arbitrary request-level endpoint overrides.
    /// </summary>
    VKResult ValidateEndpoint(string? endpoint);

    /// <summary>
    /// Validates whether the given API Key matches an authorized BYOK credential.
    /// </summary>
    VKResult ValidateApiKey(VKSensitiveString? apiKey);
}
