using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Corpus.Filtering.Internal;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Hook class for registering Filtering filters and stages.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class FilteringFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKFilteringOptions options)
    {
        // Register the filtering stage
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, CorpusFilteringStage>());

        // 0. Stickiness Bypass (Evaluated first to force keep sticky entries)
        if (options.EnableStickinessFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, StickinessFilter>());

        // 1. Static Metadata Filters (Cheap)
        if (options.EnablePersonaFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, PersonaFilter>());

        if (options.EnableUserSegmentFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, UserSegmentFilter>());

        if (options.EnableFreshnessFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, FreshnessFilter>());

        if (options.EnableScheduleFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, ScheduleFilter>());

        if (options.EnableDelayFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, DelayFilter>());

        // 2. Behavioral/State Gated Filters
        if (options.EnableDependencyFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, DependencyFilter>());

        if (options.EnableCooldownFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, CooldownFilter>());

        if (options.EnableProbabilityFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, ProbabilityFilter>());

        if (options.EnableEmotionGatedFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, EmotionGatedFilter>());

        if (options.EnableRevealFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, RevealFilter>());

        if (options.EnableMaxCountFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, MaxCountFilter>());

        if (options.EnableGroupFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, GroupFilter>());

        if (options.EnableExclusionFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, ExclusionFilter>());

        // 3. Mutex & Pruning Filters (Stateful)
        if (options.EnableConflictResolutionFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, ConflictResolutionFilter>());

        if (options.EnableExclusiveGroupFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, ExclusiveGroupFilter>());

        // 4. Budget & Decay (Expensive)
        if (options.EnableTokenBudgetFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, TokenBudgetFilter>());

        if (options.EnableRecencyBiasFilter)
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKKnowledgeLifecycleFilter, RecencyBiasFilter>());
    }

    static partial void ValidateCustom(VKFilteringOptions options, List<string> failures)
    {
        if (options.DefaultCooldownTurns < -1)
        {
            failures.Add("DefaultCooldownTurns must be -1 (single-fire) or non-negative.");
        }

        if (options.RecencyDecayFactor is < 0.0 or > 1.0)
        {
            failures.Add("RecencyDecayFactor must be between 0.0 and 1.0.");
        }

        if (options.MaxEntriesPerTurn is < 1)
        {
            failures.Add("MaxEntriesPerTurn must be at least 1.");
        }

        if (options.DefaultProbability is < 0.0 or > 1.0)
        {
            failures.Add("DefaultProbability must be between 0.0 and 1.0.");
        }

        if (options.DefaultStickyTurns is < -1)
        {
            failures.Add("DefaultStickyTurns must be -1 (permanent) or non-negative.");
        }

        if (options.MaxStickyEntries is < 1)
        {
            failures.Add("MaxStickyEntries must be at least 1.");
        }
    }
}
