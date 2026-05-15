using System;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using VK.Blocks.AI.VectorStore.Sqlite.VectorStore.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the SQLite database feature.
/// Following BB.03 sequence and AP.02 idempotency.
/// </summary>
internal static class AIVectorStoreSqliteRegistration
{
    internal static IVKAIVectorStoreBuilder Register(
        IVKAIVectorStoreBuilder builder,
        Func<VKAIVectorStoreSqliteOptions, VKAIVectorStoreSqliteOptions>? transform)
    {
        // 0. [BB.03] Strategy Check (Root-Driven Switching)
        var rootOptions = builder.Services.GetVKServiceInstance<VKAIVectorStoreOptions>();
        if (rootOptions?.Type != VKAIVectorStoreType.Sqlite)
        {
            return builder;
        }

        // 1. Check-Self (Idempotency)
        if (builder.Services.IsVKBlockRegistered<VKAIVectorStoreSqliteBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = builder.Services.AddVKBlockOptions<VKAIVectorStoreSqliteOptions>(builder.Configuration, transform);

        // 3. Mark-Self
        builder.Services.AddVKBlockMarker<VKAIVectorStoreSqliteBlock>();

        // 4. Options Validation
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKAIVectorStoreSqliteOptions>, AIVectorStoreSqliteOptionsValidator>();

        // 5. Diagnostics
        // (ActivitySource and Meter are handled by source gen from the attribute on the Diagnostics class)

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services (Using TryAdd pattern - AP.02)
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorStore, AIVectorStoreSqliteDatabase>();

        builder.Services.AddResiliencePipeline("AI.VectorStore.Sqlite", (resilienceBuilder) =>
        {
            resilienceBuilder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            resilienceBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(15)
            });
        });

        return builder;
    }
}
