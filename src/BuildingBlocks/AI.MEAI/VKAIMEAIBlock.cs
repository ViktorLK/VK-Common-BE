using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.MEAI.Chat.Internal;
using VK.Blocks.AI.MEAI.Common.DependencyInjection.Internal;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Architectural marker and entry point for the VK.Blocks.AI.MEAI building block.
/// Adheres to BB.02, BB.03, AP.02, and AP.06.
/// </summary>
// [SG Marker]
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock), typeof(VKResilienceBlock)])]
public sealed partial class VKAIMEAIBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKAIMEAIBuilder builder)
    {
        var services = builder.Services;

        // 0. Register VK.Blocks.Resilience for named transport pipelines (OWN-TRANSPORT-RESILIENCE, MEA-06)
        services.AddVKResilienceBlock(builder.Configuration);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAIMEAIOptions>, MEAIStartupValidator>());

        // 1. TimeProvider singleton default (CS.06)
        services.TryAddSingleton(TimeProvider.System);

        // 2. Resilient HttpClient for AI transport with per-provider transport resilience pipeline isolation (OWN-TRANSPORT-RESILIENCE, MEA-06)
        ConfigureResilientHttpClient(services, VKAIMEAIDiagnosticsConstants.HttpClientName);
        foreach (var providerType in Enum.GetValues<VKAIProviderType>())
        {
            ConfigureResilientHttpClient(services, VKAIMEAIDiagnosticsConstants.GetHttpClientName(providerType));
        }

        // 3. Register all default capability slices via their respective [VKFeature] hubs (BB.06)
        builder.AddVKAIMEAIDefaultFeatures();
    }

    internal static void EnsureChatClientRegistered(IServiceCollection services)
        => TryAddDefaultAndKeyedClient<IChatClient>(
            services,
            static (providerOptions, options, sp) => AIMEAIClientFactory.CreateChatClient(providerOptions, options.ChainSettings, sp));

    internal static void TryAddDefaultAndKeyedClient<TClient>(
        IServiceCollection services,
        Func<VKAIMEAIProviderSettings, VKAIMEAIOptions, IServiceProvider, TClient> factory)
        where TClient : class
    {
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<VKAIMEAIOptions>>().Value;
            var providerOptions = ResolveProviderOptions(options, null);
            return factory(providerOptions, options, sp);
        });

        services.TryAddKeyedSingleton<TClient>(KeyedService.AnyKey, (sp, key) =>
        {
            var options = sp.GetRequiredService<IOptions<VKAIMEAIOptions>>().Value;
            var providerOptions = ResolveProviderOptions(options, key);
            return factory(providerOptions, options, sp);
        });
    }

    private static void ConfigureResilientHttpClient(IServiceCollection services, string clientName)
    {
        // [OR.03]
        services.AddHttpClient(clientName)
            .AddHttpMessageHandler(sp => new AIMEAITransportResilienceHandler(clientName, sp));
    }

    internal static void ReplaceIfFallbackOrAddSingleton<TInterface, TImplementation>(IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var descriptor = System.Linq.Enumerable.FirstOrDefault(services, s => s.ServiceType == typeof(TInterface));
        if (descriptor is null)
        {
            services.AddSingleton<TInterface, TImplementation>();
            return;
        }

        var isFallback = descriptor.ImplementationType?.Name.StartsWith("NoOp", StringComparison.OrdinalIgnoreCase) == true ||
                         descriptor.ImplementationType?.Name.StartsWith("Default", StringComparison.OrdinalIgnoreCase) == true;
        if (isFallback)
        {
            services.Replace(ServiceDescriptor.Singleton<TInterface, TImplementation>());
        }
    }

    internal static void ReplaceChatEnginePreservingDecorator(IServiceCollection services)
    {
        var descriptor = System.Linq.Enumerable.FirstOrDefault(services, s => s.ServiceType == typeof(IVKChatEngine));
        if (descriptor is null)
        {
            services.AddScoped<IVKChatEngine, DefaultChatEngine>();
            return;
        }

        // Check if existing descriptor is already decorating (e.g. VKGovernanceChatDecorator factory)
        var isDecorator = descriptor.ImplementationFactory is not null &&
                          descriptor.ImplementationType is null &&
                          descriptor.ImplementationInstance is null;

        if (isDecorator)
        {
            // Decorator factory already present, register DefaultChatEngine as a fallback / inner resolve candidate
            services.TryAddScoped<DefaultChatEngine>();
            return;
        }

        services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, DefaultChatEngine>());
    }

    internal static VKAIMEAIProviderSettings ResolveProviderOptions(VKAIMEAIOptions options, object? serviceKey)
    {
        VKGuard.NotNull(options);

        if (options.ProviderSettings.Count == 0)
        {
            throw new InvalidOperationException("No AI providers configured in VKAIMEAIOptions.Providers.");
        }

        if (serviceKey is not null)
        {
            // 1. Direct key match (e.g. "default", "ollama", "azure")
            var keyStr = serviceKey.ToString();
            if (!string.IsNullOrWhiteSpace(keyStr) && options.ProviderSettings.TryGetValue(keyStr, out var directMatch))
            {
                return directMatch;
            }

            // 2. VKAIProviderType enum match (either enum object or matching string)
            VKAIProviderType? targetType = serviceKey is VKAIProviderType pt ? pt :
                (Enum.TryParse<VKAIProviderType>(keyStr, ignoreCase: true, out var parsed) ? parsed : null);

            if (targetType.HasValue)
            {
                foreach (var (_, provOpt) in options.ProviderSettings)
                {
                    if (provOpt.Provider == targetType.Value)
                    {
                        return provOpt;
                    }
                }
            }
        }

        // 3. Fallback to DefaultProvider
        if (options.ProviderSettings.TryGetValue(options.DefaultProvider, out var defaultOpt))
        {
            return defaultOpt;
        }

        // 4. First configured provider
        return System.Linq.Enumerable.First(options.ProviderSettings.Values);
    }

    // [SG Hook]
    static partial void ValidateBlockCustom(VKAIMEAIOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.ProviderSettings.Count > 0 && !options.ProviderSettings.ContainsKey(options.DefaultProvider))
        {
            failures.Add($"Default provider '{options.DefaultProvider}' must be present in configured Providers dictionary.");
        }

        foreach (var (key, providerOpt) in options.ProviderSettings)
        {
            if (!Enum.IsDefined(typeof(VKAIProviderType), providerOpt.Provider))
            {
                failures.Add($"Provider '{key}' has an undefined provider type '{(int)providerOpt.Provider}'.");
            }

            if (providerOpt.Provider == VKAIProviderType.AzureOpenAI && string.IsNullOrWhiteSpace(providerOpt.Endpoint))
            {
                failures.Add($"Provider '{key}' configured as AzureOpenAI requires a non-empty Endpoint.");
            }
        }
    }
}
