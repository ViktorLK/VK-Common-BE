using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Web.CorrelationId.Internal;
using VK.Blocks.Web.ProblemDetails.Internal;
using VK.Blocks.Web.UserContext.Internal;

namespace VK.Blocks.Web.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Web building block.
/// Complies with BB.03 (Internal Core Registration Sequence).
/// </summary>
internal static class WebBlockRegistration
{
    public static IVKBlockBuilder<VKWebBlock> Register(IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        var builder = new VKBlockBuilder<VKWebBlock>(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKWebBlock>())
        {
            return builder;
        }

        // 2. Options Registration (Web block itself currently has no root options, but we register marker)
        // 3. Mark-Self
        services.AddVKBlockMarker<VKWebBlock>();

        // 4. Options Validation (N/A for root Web block)

        // 5. Diagnostics/Static Metadata (Handled by VKBlockMarker attribute)

        // 6. Feature Toggle (Web is always enabled if registered)

        // 7. Core Services
        RegisterCoreWebServices(services);

        return builder;
    }

    public static void RegisterSecurityDiscoveryServices(IServiceCollection services)
    {
        // Register the controller manually since it is internal
        services.AddControllers()
            .AddApplicationPart(typeof(VK.Blocks.Web.Discovery.Internal.SecurityDiscoveryController).Assembly)
            .AddControllersAsServices();
    }

    public static void RegisterCorsServices(IServiceCollection services, VKCorsOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        services.AddCors(opt =>
        {
            opt.AddPolicy(VKCorsOptions.DefaultPolicyName, policy =>
            {
                if (options.AllowedOrigins.Length > 0)
                {
                    if (options.AllowedOrigins.Contains("*"))
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(options.AllowedOrigins);
                    }
                }

                if (options.AllowedMethods.Length > 0)
                {
                    if (options.AllowedMethods.Contains("*"))
                    {
                        policy.AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithMethods(options.AllowedMethods);
                    }
                }

                if (options.AllowedHeaders.Length > 0)
                {
                    if (options.AllowedHeaders.Contains("*"))
                    {
                        policy.AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithHeaders(options.AllowedHeaders);
                    }
                }

                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }

                if (options.ExposedHeaders.Length > 0)
                {
                    policy.WithExposedHeaders(options.ExposedHeaders);
                }
            });
        });
    }

    public static void RegisterCorrelationIdServices(IServiceCollection services)
    {
        services.TryAddScoped<IVKCorrelationIdProvider, DefaultCorrelationIdProvider>();
    }

    public static IVKBlockBuilder<VKWebBlock> WithFeatureInternal<TOptions>(
        this IVKBlockBuilder<VKWebBlock> builder,
        Func<TOptions> registerOptionsFunc,
        Action<TOptions>? registerServicesAction = null)
        where TOptions : class, new()
    {
        TOptions options = registerOptionsFunc();
        registerServicesAction?.Invoke(options);
        return builder;
    }

    private static void RegisterCoreWebServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Problem Details implementation
        services.AddScoped<Microsoft.AspNetCore.Diagnostics.IExceptionHandler, ExceptionHandler>();
        services.TryAddSingleton<IVKProblemDetailsFactory, DefaultProblemDetailsFactory>();

        // TODO: [Mapping SG] Remove this manual registration once Source Generator auto-registration is implemented.
        services.TryAddSingleton<IVKMapper<VKErrorResponse, VKWebProblemDetails>, ExceptionProblemDetailsMapper>();

        // User Context
        services.TryAddScoped<IVKUserContext, HttpContextUserContext>();
    }
}

