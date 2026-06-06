using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.DependencyInjection.Internal;

/// <summary>
/// Internal central registration logic for AI Psyche (BB.03.1).
/// </summary>
internal static class AIPsycheBlockRegistration
{
    public static IVKAIPsycheBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIPsycheOptions, VKAIPsycheOptions>? transform = null)
    {
        var builder = new AIPsycheBlockBuilder(services, configuration.GetSection(VKBlocksConstants.VKBlocksConfigPrefix));

        if (services.IsVKBlockRegistered<VKAIPsycheBlock>())
        {
            return builder;
        }
        VKAIPsycheOptions options = services.AddVKBlockOptions(configuration, transform);

        services.AddVKBlockMarker<VKAIPsycheBlock>();

        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
