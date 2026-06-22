using System;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using VK.Blocks.VectorStore.Sqlite.SqliteVec.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite.Common.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the SQLite database feature.
/// Following BB.03 sequence and AP.02 idempotency.
/// </summary>
internal static class VectorStoreSqliteBlockRegistration
{
    internal static IVKVectorStoreBuilder Register(
        IVKVectorStoreBuilder builder,
        Func<VKVectorStoreSqliteOptions, VKVectorStoreSqliteOptions>? transform)
    {
        // 0. [BB.03] Strategy Check (Root-Driven Switching)
        var rootDefaultsOptions = builder.Services.GetVKServiceInstance<VKVectorStoreDefaultsOptions>();
        if (rootDefaultsOptions?.Type != VKVectorStoreType.Sqlite)
        {
            return builder;
        }

        // 1. Check-Self (Idempotency)
        if (builder.Services.IsVKBlockRegistered<VKVectorStoreSqliteBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = builder.Services.AddVKBlockOptions<VKVectorStoreSqliteOptions>(builder.Configuration, transform);
        builder.Services.AddVKBlockOptions<VKVectorStoreSqliteDefaultsOptions>(builder.Configuration);

        // 3. Mark-Self
        builder.Services.AddVKBlockMarker<VKVectorStoreSqliteBlock>();

        // 4. Options Validation
        // builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKVectorStoreSqliteOptions>, AIVectorStoreSqliteOptionsValidator>();

        // 5. Diagnostics
        // (ActivitySource and Meter are handled by source gen from the attribute on the Diagnostics class)

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services (Using TryAdd pattern - AP.02)
        builder.WithScoped<VKVectorStoreBlock, IVKVectorStore, SqliteVectorStore>();

        builder.Services.AddResiliencePipeline("VectorStore.Sqlite", (resilienceBuilder) =>
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
