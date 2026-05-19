using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Extension methods for AI connection resolution.
/// [AP.05] Hierarchical Configuration Pattern.
/// </summary>
public static class VKAIConnectionExtensions
{
    /// <summary>
    /// Resolves the final connection contract by merging baseline settings and incremental overrides.
    /// [AP.05] Priority: Overrides > Settings.
    /// </summary>
    /// <param name="settings">The baseline connection settings (e.g., from Options).</param>
    /// <param name="overrides">The optional request-level overrides (e.g., from Args).</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the resolved contract or an error.</returns>
    public static VKResult<VKAIResolvedContract> ResolveConnection(
        this IVKAIProviderOptions settings,
        VKAIDefaultsOptions globalOptions,
        IVKAIProviderOverrides? overrides = null)
    {
        VKGuard.NotNull(settings);
        VKGuard.NotNull(globalOptions);

        // [AP.05] Merge using null-coalescing priority: args?.Property ?? _options.Property ?? _globalOptions.Property
        var provider = overrides?.Provider ?? settings.Provider ?? globalOptions.Provider;
        var modelId = overrides?.ModelId ?? settings.ModelId;
        var apiKey = overrides?.ApiKey ?? settings.ApiKey;
        var endpoint = overrides?.Endpoint ?? settings.Endpoint;

        // [CS.01] Validation at the boundary
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return VKResult.Failure<VKAIResolvedContract>(VKAIErrors.InvalidModel);
        }

        if (apiKey is null || apiKey.Value.IsEmpty)
        {
            return VKResult.Failure<VKAIResolvedContract>(VKAIErrors.AuthenticationFailed);
        }

        return VKResult.Success(new VKAIResolvedContract(
            Provider: provider,
            ModelId: modelId,
            ApiKey: apiKey.Value,
            Endpoint: endpoint
        ));
    }
}
