using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Retry;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.Common.DependencyInjection;
using VK.Blocks.AI.SemanticKernel;
using VK.Blocks.AI.VectorStore;
using VK.Blocks.AI.VectorStore.Sqlite;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling;
using VK.Blocks.MultiTenancy;
using VK.Blocks.MultiTenancy.Web;
using VK.Blocks.Persistence.EFCore;
using VK.Blocks.Persistence.Sqlite;
using VK.Blocks.Web;
using VK.Labs.PersonaWeavePulsar.Directive.Internal;
using VK.Labs.PersonaWeavePulsar.Echo;
using VK.Labs.PersonaWeavePulsar.Echo.Internal;
using VK.Labs.PersonaWeavePulsar.Knowledge.Internal;
using VK.Labs.PersonaWeavePulsar.Persona;
using VK.Labs.PersonaWeavePulsar.Persona.Internal;
using VK.Labs.PersonaWeavePulsar.TenantConfig;
using VK.Labs.PersonaWeavePulsar.TenantConfig.Internal;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection;

internal static class PwpServiceRegistration
{
    internal static IPwpBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<PwpOptions, PwpOptions>? configure = null)
    {
        VKGuard.NotNull(services);

        // 1. Options Registration (Need this early for feature toggles)
        var options = configuration.GetSection("Pwp").Get<PwpOptions>() ?? new PwpOptions();
        if (configure != null)
        {
            options = configure(options);
        }
        services.AddSingleton(options);

        // 2. Register PWP Specific Implementations FIRST to win over TryAdd in Building Blocks
        RegisterFeatures(services, options);

        // 3. Core Infrastructure
        services.AddVKCoreBlock(configuration);
        services.AddPersistenceEFCoreBlock<PwpDbContext>(configuration, conf => conf with
        {
            EnableMultiTenancy = true
        });
        services.AddPersistenceSqliteBlock<PwpDbContext>(configuration, conf => conf with
        {
            ConnectionString = options.HistoryConnection
        });
        services.AddDataProtection();

        IPwpBuilder builder = new PwpBuilder(services, configuration);

        // 3.5 MultiTenancy (Registered before Web block to ensure context is available)
        services.AddScoped<IVKTenantStore, Common.Internal.PwpTenantStore>();
        services.AddMultiTenancyBlock(configuration, conf => conf with
        {
            Enabled = true,
            RequireTenant = true
        });
        services.AddMultiTenancyWebBlock(configuration, conf => conf with
        {
            Enabled = true,
            TenantRouteKey = "tenantId",
            TenantHeaderName = "X-Tenant-Id"
        });

        // 4. Web Infrastructure Registration
        services.AddExceptionHandlingBlock(configuration);

        services.AddVKWebBlock(configuration)
                .WithCorrelationId(configuration)
                .WithRequestLogging(configuration);

        // 5. AI Infrastructure Registration
        services.AddVKAIBlock(configuration)
                .AddVKAIDefaultFeatures();

        services.AddVKAIVectorStoreBlock(configuration)
                .AddVKAISqliteDatabase();

        services.AddVKPsycheBlock(configuration)
                .AddVKDefaultFeatures();

        services.AddVKAISKBlock(configuration)
                .WithKernelCaching();

        // 5.1 Decorate the Persona Renderer (SRP + L3 Markdown injection)
        services.Decorate<IVKPersonaRenderer, PwpPersonaRendererDecorator>();
        services.AddScoped<Internal.PwpContext>();
        services.AddScoped<VKAISKDefaultOptionsProvider>();
        services.AddScoped<IVKAISKOptionsProvider, Internal.PwpAISKOptionsProvider>();
        services.AddScoped<IVKChatOptionsProvider, Internal.PwpChatOptionsProvider>();

        // 6. Resilience Pipelines (Required by History Store)
        services.AddResiliencePipeline("AI.VectorStore.Sqlite", (resilienceBuilder) =>
        {
            resilienceBuilder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });
        });

        return builder;
    }

    private static void RegisterFeatures(IServiceCollection services, PwpOptions options)
    {
        // 1. Orchestration (PWP09) - CORE (Always On)
        services.AddScoped<IVKDirectiveStore, PwpSqliteDirectiveStore>();
        services.TryAddScoped<IPwpIntentDetectionSlice, PwpIntentDetectionSlice>();
        services.TryAddScoped<IPwpPersonaLoaderSlice, PwpPersonaLoaderSlice>();
        services.TryAddScoped<IPwpLorebookInterceptor, PwpLorebookInterceptor>();
        services.TryAddScoped<PwpChatEngine>();

        // 2. Personas & Lore - CORE (Always On)
        // Use Replace to ensure our SQLite implementation wins if already registered,
        // OR register here knowing that the BuildingBlocks will use TryAdd later.
        services.AddScoped<IVKKnowledgeStore, PwpSqliteKnowledgeStore>();
        services.AddScoped<IVKPersonaStore, PwpSqlitePersonaStore>();

        // 3. Memory & Settings - CORE (Always On)
        services.TryAddScoped<IPwpChatHistoryStore, PwpSqliteChatHistoryStore>();
        services.TryAddScoped<IPwpTenantConfigStore, PwpSqliteTenantConfigStore>();
        services.TryAddScoped<IPwpPromptPresetStore, PwpSqlitePromptPresetStore>();
        services.AddScoped<IVKEchoStore, PwpMemoryEchoes>();
        services.TryAddScoped<IPwpUserPersonaStore, PwpSqliteUserPersonaStore>();

        // 4. BioFeedback - EXTENDED
        /* Somatic logic commented out
        if (options.Features.EnableBioFeedback)
        {
            // BioFeedback folder missing implementation, but keeping placeholder
            // services.TryAddSingleton<BioFeedback.PwpBioSensorAdapter>();
        }
        */

        // 5. Pulsar / Diagnostics - EXTENDED
        if (options.Features.EnableDetailedDiagnostics)
        {
            services.TryAddSingleton<IPwpDiagnosticsManager, PwpDiagnosticsManager>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<Microsoft.SemanticKernel.IFunctionInvocationFilter, PwpSKThoughtPathHook>());
        }

        // 6. Reflex (Stress & Proactive) - EXTENDED
        /* Somatic logic commented out
        services.TryAddSingleton<IPwpStressMonitor, Reflex.Internal.PwpStressMonitor>();
        if (options.Features.EnableProactiveReflex)
        {
            services.AddHostedService<Reflex.Internal.PwpProactiveEngine>();
        }
        */

        // 7. Monitoring (Phase 3) - EXTENDED
        /* Somatic logic commented out
        if (options.Features.EnableSelfMonitor)
        {
            services.TryAddScoped<IPwpSelfMonitor, Monitoring.Internal.PwpSelfMonitor>();
        }
        else
        {
            services.TryAddScoped<IPwpSelfMonitor, Monitoring.Internal.NoOpPwpSelfMonitor>();
        }
        */
    }
}
