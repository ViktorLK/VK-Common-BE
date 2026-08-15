using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Security guard service responsible for validating connection overrides (SSRF protection & BYOK ownership).
/// </summary>
public interface IVKConnectionValidator
{
    /// <summary>
    /// Validates whether the given endpoint URI is in the permitted allowlist for the specified tenant.
    /// Prevents SSRF attacks from arbitrary request-level endpoint overrides.
    /// </summary>
    VKResult ValidateEndpoint(VKTenantId tenantId, string? endpoint);

    /// <summary>
    /// Validates whether the given API Key matches an authorized BYOK credential registered for the tenant.
    /// </summary>
    VKResult ValidateApiKey(VKTenantId tenantId, VKSensitiveString? apiKey);
}
