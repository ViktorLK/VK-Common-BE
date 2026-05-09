using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Web.DependencyInjection.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// Extension methods for configuring the Web building block.
/// Complies with AP.02 (Wrapper Pattern) and AP.03 (Level 1 Public API).
/// </summary>
public static class VKWebBlockExtensions
{
    /// <summary>
    /// Adds the Web building block to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>A builder to configure the Web block.</returns>
    public static IVKBlockBuilder<VKWebBlock> AddVKWebBlock(this IServiceCollection services, IConfiguration configuration)
    {
        return WebBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Configures Correlation ID for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithCorrelationId(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration configuration)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNull(configuration);
        return builder.WithCorrelationId(options => builder.Services.AddVKBlockOptions<VKCorrelationIdOptions>(configuration));
    }

    /// <summary>
    /// Configures Correlation ID for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithCorrelationId(this IVKBlockBuilder<VKWebBlock> builder, Func<VKCorrelationIdOptions, VKCorrelationIdOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return WebBlockRegistration.WithFeatureInternal(builder, () => builder.Services.AddVKBlockOptions(builder.Configuration, transform), _ => WebBlockRegistration.RegisterCorrelationIdServices(builder.Services));
    }

    /// <summary>
    /// Configures Security Discovery (Diagnostics) for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithSecurityDiscovery(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration configuration)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNull(configuration);
        return builder.WithSecurityDiscovery(options => builder.Services.AddVKBlockOptions<VKSecurityDiscoveryOptions>(configuration));
    }

    /// <summary>
    /// Configures Security Discovery (Diagnostics) for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithSecurityDiscovery(this IVKBlockBuilder<VKWebBlock> builder, Func<VKSecurityDiscoveryOptions, VKSecurityDiscoveryOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return WebBlockRegistration.WithFeatureInternal(builder, () => builder.Services.AddVKBlockOptions(builder.Configuration, transform), _ => WebBlockRegistration.RegisterSecurityDiscoveryServices(builder.Services));
    }

    /// <summary>
    /// Configures Request Logging for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithRequestLogging(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration? configuration = null)
    {
        VKGuard.NotNull(builder);
        return builder.WithRequestLogging(options => builder.Services.AddVKBlockOptions<VKRequestLoggingOptions>(configuration ?? builder.Configuration));
    }

    /// <summary>
    /// Configures Request Logging for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithRequestLogging(this IVKBlockBuilder<VKWebBlock> builder, Func<VKRequestLoggingOptions, VKRequestLoggingOptions>? transform)
    {
        VKGuard.NotNull(builder);
        return WebBlockRegistration.WithFeatureInternal(builder, () => builder.Services.AddVKBlockOptions(builder.Configuration, transform));
    }

    /// <summary>
    /// Configures Security Headers for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithSecurityHeaders(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration? configuration = null)
    {
        VKGuard.NotNull(builder);
        return builder.WithSecurityHeaders(options => builder.Services.AddVKBlockOptions<VKSecurityHeadersOptions>(configuration ?? builder.Configuration));
    }

    /// <summary>
    /// Configures Security Headers for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithSecurityHeaders(this IVKBlockBuilder<VKWebBlock> builder, Func<VKSecurityHeadersOptions, VKSecurityHeadersOptions>? transform)
    {
        VKGuard.NotNull(builder);
        return WebBlockRegistration.WithFeatureInternal(builder, () => builder.Services.AddVKBlockOptions(builder.Configuration, transform));
    }

    /// <summary>
    /// Configures CORS for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithCors(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration? configuration = null)
    {
        VKGuard.NotNull(builder);
        return builder.WithCors(options => builder.Services.AddVKBlockOptions<VKCorsOptions>(configuration ?? builder.Configuration));
    }

    /// <summary>
    /// Configures CORS for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithCors(this IVKBlockBuilder<VKWebBlock> builder, Func<VKCorsOptions, VKCorsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return WebBlockRegistration.WithFeatureInternal(builder, () => builder.Services.AddVKBlockOptions(builder.Configuration, transform), options => WebBlockRegistration.RegisterCorsServices(builder.Services, options));
    }

    /// <summary>
    /// Enables Tenant Identification for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithTenantIdentification(this IVKBlockBuilder<VKWebBlock> builder)
    {
        VKGuard.NotNull(builder);
        return builder;
    }

    /// <summary>
    /// Enables Response Shaping support (?fields=...) for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithResponseShaping(this IVKBlockBuilder<VKWebBlock> builder)
    {
        VKGuard.NotNull(builder);
        builder.Services.AddControllers(opt =>
        {
            opt.Filters.Add<VK.Blocks.Web.Shaping.Internal.ResponseShapingFilter>();
        });

        return builder;
    }
}

