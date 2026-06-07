using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Psyche builder.
/// </summary>
internal sealed class AIPsycheBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    : VKBlockBuilder<VKAIPsycheBlock>(services, configuration!), IVKAIPsycheBuilder;
