using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

// [AP.01] sealed
internal sealed class DefaultAIEngineAccessor : IVKAIEngineAccessor
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultAIEngineAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    public IVKChatEngine? GetChatEngine(VKAIProviderType providerType)
    {
        return _serviceProvider.GetKeyedService<IVKChatEngine>(providerType)
            ?? _serviceProvider.GetKeyedService<IVKChatEngine>(providerType.ToString());
    }

    public IVKChatEngine? GetChatEngine(string providerName)
    {
        VKGuard.NotNullOrWhiteSpace(providerName);
        if (Enum.TryParse<VKAIProviderType>(providerName, true, out var providerType))
        {
            var engine = _serviceProvider.GetKeyedService<IVKChatEngine>(providerType);
            if (engine is not null)
            {
                return engine;
            }
        }

        return _serviceProvider.GetKeyedService<IVKChatEngine>(providerName);
    }

    public T? GetEngine<T>(object serviceKey) where T : class
    {
        VKGuard.NotNull(serviceKey);
        return _serviceProvider.GetKeyedService<T>(serviceKey);
    }
}
