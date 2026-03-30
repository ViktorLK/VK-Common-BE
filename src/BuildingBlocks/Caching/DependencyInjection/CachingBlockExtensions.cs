using VK.Blocks.Caching.Abstractions;
using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Caching.Options;
using VK.Blocks.Caching.Providers.Hybrid;
using VK.Blocks.Caching.Providers.Memory;
using VK.Blocks.Caching.Providers.Redis;
using VK.Blocks.Caching.Providers.SqlServer;
using VK.Blocks.Caching.Resilience.Avalanche;
using VK.Blocks.Caching.Resilience.Breakdown;
using VK.Blocks.Caching.Resilience.Penetration;
using VK.Blocks.Caching.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

using Polly;
using Polly.Wrap;

namespace VK.Blocks.Caching.DependencyInjection;

public static class CachingBlockExtensions
{
    public static IServiceCollection AddVKBlocksCaching(
        this IServiceCollection services,
        Action<CachingOptions>? configureOptions = null)
    {
        services.Configure(configureOptions ?? (_ => { }));
        services.AddOptions<ResilienceOptions>()
            .Configure<IConfiguration>((options, configuration) => 
                configuration.GetSection("Caching:Resilience").Bind(options));

        services.TryAddSingleton<ICacheSerializer, JsonCacheSerializer>();
        services.TryAddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
        services.TryAddSingleton<IJitterExpiryStrategy, JitterExpiryStrategy>();
        services.TryAddSingleton<ILockBasedGuard, LockBasedGuard>();
        services.TryAddSingleton<INullValueGuard, NullValueGuard>();
        services.TryAddSingleton<ICacheBlock, CacheBlock>();

        // We use a temporary options object to decide which provider to register at startup.
        var options = new CachingOptions();
        configureOptions?.Invoke(options);

        switch (options.Provider)
        {
            case CacheProviderType.Redis:
                AddRedisProvider(services);
                break;
            case CacheProviderType.Hybrid:
                AddHybridProvider(services);
                break;
            case CacheProviderType.SqlServer:
                AddSqlServerProvider(services);
                break;
            case CacheProviderType.Memory:
            default:
                AddMemoryProvider(services);
                break;
        }

        return services;
    }

    private static void AddMemoryProvider(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();
        services.TryAddSingleton<IDistributedLockProvider, MemoryDistributedLockProvider>();
    }

    private static void AddSqlServerProvider(IServiceCollection services)
    {
        // Requires IDistributedCache to be registered externally (e.g. AddDistributedSqlServerCache)
        services.TryAddSingleton<ICacheProvider, SqlServerCacheProvider>();
        // Fallback lock provider for SQL Server if no distributed lock is available
        services.TryAddSingleton<IDistributedLockProvider, MemoryDistributedLockProvider>();
    }

    private static void AddRedisProvider(IServiceCollection services)
    {
        services.AddOptions<RedisCacheOptions>();
        AddResiliencePolicies(services);
        
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            var configuration = ConfigurationOptions.Parse(redisOptions.Configuration);
            // NF-04: Using Connect to minimize blocking impact during startup.
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
        services.TryAddSingleton<IDistributedLockProvider, RedisDistributedLockProvider>();
    }

    private static void AddResiliencePolicies(IServiceCollection services)
    {
        var retryPolicy = Policy
            .Handle<RedisException>()
            .Or<RedisConnectionException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = Policy
            .Handle<RedisException>()
            .Or<RedisConnectionException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddSingleton<AsyncPolicyWrap>(retryPolicy.WrapAsync(circuitBreakerPolicy));
    }

    private static void AddHybridProvider(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddOptions<RedisCacheOptions>();
        services.AddOptions<HybridCacheOptions>();
        AddResiliencePolicies(services);

        services.AddSingleton<MemoryCacheProvider>();
        services.AddSingleton<RedisCacheProvider>();

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            var configuration = ConfigurationOptions.Parse(redisOptions.Configuration);
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.TryAddSingleton<ICacheProvider>(sp =>
            new HybridCacheProvider(
                sp.GetRequiredService<MemoryCacheProvider>(),
                sp.GetRequiredService<RedisCacheProvider>(),
                sp.GetRequiredService<IOptions<HybridCacheOptions>>(),
                sp.GetRequiredService<IConnectionMultiplexer>()));

        services.TryAddSingleton<IDistributedLockProvider, RedisDistributedLockProvider>();
    }
}


