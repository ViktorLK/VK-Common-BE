using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Web.CorrelationId.Internal;
using VK.Blocks.Web.ProblemDetails.Internal;
using VK.Blocks.Web.UserContext.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// A marker type for the VK.Blocks.Web building block.
/// Complies with BB.02 (IVKBlockMarker).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock), typeof(VKExceptionHandlingBlock)])]
public sealed partial class VKWebBlock
{

    static partial void RegisterBlockCustom(IVKWebBuilder builder)
    {
        RegisterCoreWebServices(builder.Services);

        // Register the controller manually since it is internal
        builder.Services.AddControllers()
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


    private static void RegisterCoreWebServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Problem Details implementation
        services.AddScoped<Microsoft.AspNetCore.Diagnostics.IExceptionHandler, ExceptionHandler>();
        services.TryAddSingleton<IVKProblemDetailsFactory, DefaultProblemDetailsFactory>();

        // TODO: [Mapping SG] Remove this manual registration once Source Generator auto-registration is implemented.
        services.TryAddSingleton<IVKMapper<VKErrorResponse, VKWebProblemDetails>, ExceptionProblemDetailsMapper>();

        // User / Security Context (Override Core's fallback DefaultSecurityContext/DefaultIdentityContext)
        services.RemoveAll<IVKSecurityContext>();
        services.RemoveAll<IVKIdentityContext>();
        services.AddScoped<IVKSecurityContext, HttpContextUserContext>();
        services.AddScoped<IVKIdentityContext>(sp => sp.GetRequiredService<IVKSecurityContext>());
    }

    public static void RegisterRequestBodyLimitServices(IServiceCollection services, VKRequestBodyLimitOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(opt =>
        {
            opt.Limits.MaxRequestBodySize = options.MaxRequestBodySize;
        });

        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opt =>
        {
            opt.MultipartBodyLengthLimit = options.MaxRequestBodySize;
        });
    }

    public static void RegisterGracefulShutdownServices(IServiceCollection services, VKGracefulShutdownOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        services.TryAddSingleton<VK.Blocks.Web.Shutdown.Internal.GracefulShutdownTracker>();
        services.AddHostedService<VK.Blocks.Web.Shutdown.Internal.GracefulShutdownHostedService>();
    }
}
