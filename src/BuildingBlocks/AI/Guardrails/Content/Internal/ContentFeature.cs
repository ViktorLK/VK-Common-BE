using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Guardrails.Content.Internal;

/// <summary>
/// Content Guard feature marker and registration hub.
/// </summary>
internal sealed partial class ContentFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKContentOptions options)
    {
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<IVKModerationEngine, NoOpVKModerationEngine>(services);

        Decorate<IVKChatEngine, VKGovernanceChatDecorator>(services);
        Decorate<IVKEmbeddingsEngine, VKGovernanceEmbeddingsDecorator>(services);
    }

    private static void Decorate<TInterface, TDecorator>(IServiceCollection services)
        where TDecorator : class, TInterface
        where TInterface : class
    {
        var descriptor = System.Linq.Enumerable.FirstOrDefault(services, s => s.ServiceType == typeof(TInterface));
        if (descriptor is not null)
        {
            var innerFactory = descriptor.ImplementationFactory;
            var innerType = descriptor.ImplementationType;
            var innerInstance = descriptor.ImplementationInstance;

            services.Remove(descriptor);

            services.Add(new ServiceDescriptor(typeof(TInterface), provider =>
            {
                TInterface inner;
                if (innerFactory is not null)
                {
                    inner = (TInterface)innerFactory(provider);
                }
                else if (innerInstance is not null)
                {
                    inner = (TInterface)innerInstance;
                }
                else if (innerType is not null)
                {
                    inner = (TInterface)ActivatorUtilities.CreateInstance(provider, innerType);
                }
                else
                {
                    throw new System.InvalidOperationException($"Cannot resolve inner service for {typeof(TInterface)}");
                }

                return ActivatorUtilities.CreateInstance<TDecorator>(provider, inner);
            }, descriptor.Lifetime));
        }
    }

    /// <summary>Add content-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKContentOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
