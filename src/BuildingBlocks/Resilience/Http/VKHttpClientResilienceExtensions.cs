using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Http.Internal;

namespace VK.Blocks.Resilience;

/// <summary>
/// Extension methods for configuring resilience pipelines on <see cref="IHttpClientBuilder"/>.
/// Follows [AP.01], [AP.02], [OR.03].
/// </summary>
public static class VKHttpClientResilienceExtensions
{
    /// <summary>
    /// Adds a named VK resilience pipeline handler to the HTTP client builder.
    /// </summary>
    public static IHttpClientBuilder AddVKResiliencePipeline(
        this IHttpClientBuilder builder,
        string pipelineName)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNullOrWhiteSpace(pipelineName);

        builder.AddHttpMessageHandler(sp => new VKResilienceDelegatingHandler(pipelineName, sp));
        return builder;
    }

    /// <summary>
    /// Adds a standard default VK resilience pipeline handler (Timeout + Retry + Jitter + Circuit Breaker) to the HTTP client builder.
    /// </summary>
    public static IHttpClientBuilder AddVKStandardResilienceHandler(
        this IHttpClientBuilder builder,
        string? clientName = null,
        Action<IVKPolicyBuilder>? configure = null)
    {
        VKGuard.NotNull(builder);

        var pipelineName = $"http:standard:{clientName ?? builder.Name}";

        builder.Services.AddSingleton<IStartupPostConfigure>(sp =>
        {
            var registry = sp.GetRequiredService<IVKPolicyRegistry>();
            if (!registry.TryGetPipeline(pipelineName, out _))
            {
                registry.GetOrAddPipeline(pipelineName, policyBuilder =>
                {
                    policyBuilder
                        .AddTimeout(TimeSpan.FromSeconds(30))
                        .AddRetry(maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(500), useJitter: true)
                        .AddCircuitBreaker(
                            circuitBreakerKey: $"cb:{pipelineName}",
                            durationOfBreak: TimeSpan.FromSeconds(30),
                            minimumThroughput: 10,
                            failureRatio: 0.5);

                    configure?.Invoke(policyBuilder);
                    return policyBuilder.Build();
                });
            }
            return new StartupPostConfigure();
        });

        builder.AddHttpMessageHandler(sp => new VKResilienceDelegatingHandler(pipelineName, sp));
        return builder;
    }

    private interface IStartupPostConfigure;
    private sealed class StartupPostConfigure : IStartupPostConfigure;
}
