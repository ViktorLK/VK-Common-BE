using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Wrap;
using StackExchange.Redis;

namespace VK.Blocks.Caching;

/// <summary>
/// Marker type for the Caching building block.
/// </summary>
///
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKCachingBlock
{

    static partial void RegisterBlockCustom(IVKCachingBuilder builder)
    {
        var services = builder.Services;

        services.TryAddSingleton(TimeProvider.System);

        // Retrieve option instance
        var defaults = services.GetVKServiceInstance<VKCachingOptions>();
        if (defaults is null)
        {
            return;
        }

        if (defaults.Provider == CacheProviderType.Redis || defaults.Provider == CacheProviderType.Hybrid)
        {
            AddResiliencePolicies(services);

            services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<VKCachingOptions>>().Value;
                var configuration = ConfigurationOptions.Parse(options.Redis.Configuration);
                return ConnectionMultiplexer.Connect(configuration);
            });
        }
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

        services.TryAddSingleton<AsyncPolicyWrap>(retryPolicy.WrapAsync(circuitBreakerPolicy));
    }
}
