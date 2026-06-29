using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.VectorIngest.Common.DependencyInjection.Internal;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Service collection extensions for VK.Blocks.VectorIngest module.
/// Public API Wrapper following BB.03.1.
/// </summary>
[ExcludeFromCodeCoverage]
public static class VKVectorIngestBlockExtensions
{
    /// <summary>
    /// Adds the AI Ingest building block to the service collection.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKVectorIngestBlock(
        this IServiceCollection services,
        IConfiguration configuration)
        => AIIngestBlockRegistration.Register(services, configuration);

    /// <summary>
    /// Adds the AI Ingest building block to the service collection with custom configuration.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKVectorIngestBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorIngestOptions, VKVectorIngestOptions> transform)
        => AIIngestBlockRegistration.Register(services, configuration, transform);
}
