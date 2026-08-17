using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Extension methods for configuring the Web building block.
/// Complies with AP.02 (Wrapper Pattern) and AP.03 (Level 1 Public API).
/// </summary>
public static partial class VKWebBuilderExtensions
{

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
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKCorrelationIdOptions>(builder.Configuration, transform), _ => VKWebBlock.RegisterCorrelationIdServices(builder.Services));
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
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKSecurityDiscoveryOptions>(builder.Configuration, transform));
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
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKRequestLoggingOptions>(builder.Configuration, transform));
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
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKSecurityHeadersOptions>(builder.Configuration, transform));
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
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKCorsOptions>(builder.Configuration, transform), options => VKWebBlock.RegisterCorsServices(builder.Services, options));
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

    /// <summary>
    /// Configures Request Body Size Limit for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithRequestBodyLimit(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration? configuration = null)
    {
        VKGuard.NotNull(builder);
        return builder.WithRequestBodyLimit(options => builder.Services.AddVKBlockOptions<VKRequestBodyLimitOptions>(configuration ?? builder.Configuration));
    }

    /// <summary>
    /// Configures Request Body Size Limit for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithRequestBodyLimit(this IVKBlockBuilder<VKWebBlock> builder, Func<VKRequestBodyLimitOptions, VKRequestBodyLimitOptions>? transform)
    {
        VKGuard.NotNull(builder);
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKRequestBodyLimitOptions>(builder.Configuration, transform), options => VKWebBlock.RegisterRequestBodyLimitServices(builder.Services, options));
    }

    /// <summary>
    /// Configures Graceful Shutdown and request draining for the Web block.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithGracefulShutdown(this IVKBlockBuilder<VKWebBlock> builder, IConfiguration? configuration = null)
    {
        VKGuard.NotNull(builder);
        return builder.WithGracefulShutdown(options => builder.Services.AddVKBlockOptions<VKGracefulShutdownOptions>(configuration ?? builder.Configuration));
    }

    /// <summary>
    /// Configures Graceful Shutdown and request draining for the Web block using an explicit transformation function.
    /// </summary>
    public static IVKBlockBuilder<VKWebBlock> WithGracefulShutdown(this IVKBlockBuilder<VKWebBlock> builder, Func<VKGracefulShutdownOptions, VKGracefulShutdownOptions>? transform)
    {
        VKGuard.NotNull(builder);
        return builder.WithFeatureInternal(() => builder.Services.AddVKBlockOptions<VKGracefulShutdownOptions>(builder.Configuration, transform), options => VKWebBlock.RegisterGracefulShutdownServices(builder.Services, options));
    }
}
