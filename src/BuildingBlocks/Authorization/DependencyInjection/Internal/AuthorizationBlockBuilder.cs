using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the authorization block builder.
/// </summary>
internal sealed class AuthorizationBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKAuthorizationBlock>(services, configuration), IVKAuthorizationBuilder;
