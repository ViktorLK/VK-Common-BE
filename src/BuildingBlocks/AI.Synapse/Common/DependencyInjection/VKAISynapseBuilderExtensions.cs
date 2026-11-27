using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Extension methods for customizing AI.Synapse building block registrations via <see cref="IVKAISynapseBuilder"/>.
/// Adheres to AP.06 (Anti-overprotection: explicit builder overrides).
/// </summary>
// [AP.06]
public static partial class VKAISynapseBuilderExtensions
{
    /// <summary>
    /// Overrides the default <see cref="IVKAIConnectionStore"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideConnectionStore<TStore>(this IVKAISynapseBuilder builder)
        where TStore : class, IVKAIConnectionStore
        => OverrideSingleton<IVKAIConnectionStore, TStore>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAIAffinityPinStore"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideAffinityPinStore<TStore>(this IVKAISynapseBuilder builder)
        where TStore : class, IVKAIAffinityPinStore
        => OverrideSingleton<IVKAIAffinityPinStore, TStore>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAICumulativeQuotaStore"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideCumulativeQuotaStore<TStore>(this IVKAISynapseBuilder builder)
        where TStore : class, IVKAICumulativeQuotaStore
        => OverrideSingleton<IVKAICumulativeQuotaStore, TStore>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAITokenBudgetManager"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideTokenBudgetManager<TManager>(this IVKAISynapseBuilder builder)
        where TManager : class, IVKAITokenBudgetManager
        => OverrideSingleton<IVKAITokenBudgetManager, TManager>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAIRateLimiter"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideRateLimiter<TLimiter>(this IVKAISynapseBuilder builder)
        where TLimiter : class, IVKAIRateLimiter
        => OverrideSingleton<IVKAIRateLimiter, TLimiter>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAICostLedger"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideCostLedger<TLedger>(this IVKAISynapseBuilder builder)
        where TLedger : class, IVKAICostLedger
        => OverrideSingleton<IVKAICostLedger, TLedger>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAIPriceSource"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverridePriceSource<TSource>(this IVKAISynapseBuilder builder)
        where TSource : class, IVKAIPriceSource
        => OverrideSingleton<IVKAIPriceSource, TSource>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAICostCalculator"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideCostCalculator<TCalculator>(this IVKAISynapseBuilder builder)
        where TCalculator : class, IVKAICostCalculator
        => OverrideSingleton<IVKAICostCalculator, TCalculator>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAICredentialResolver"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideCredentialResolver<TResolver>(this IVKAISynapseBuilder builder)
        where TResolver : class, IVKAICredentialResolver
        => OverrideSingleton<IVKAICredentialResolver, TResolver>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKConnectionValidator"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideConnectionValidator<TValidator>(this IVKAISynapseBuilder builder)
        where TValidator : class, IVKConnectionValidator
        => OverrideSingleton<IVKConnectionValidator, TValidator>(builder);

    /// <summary>
    /// Overrides the default <see cref="IVKAIFailurePolicy"/> implementation.
    /// </summary>
    public static IVKAISynapseBuilder OverrideFailurePolicy<TPolicy>(this IVKAISynapseBuilder builder)
        where TPolicy : class, IVKAIFailurePolicy
        => OverrideSingleton<IVKAIFailurePolicy, TPolicy>(builder);

    private static IVKAISynapseBuilder OverrideSingleton<TService, TImplementation>(IVKAISynapseBuilder builder)
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAISynapseBlock, TService, TImplementation>();
        return builder;
    }
}
