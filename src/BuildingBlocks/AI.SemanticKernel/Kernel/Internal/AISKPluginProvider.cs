using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Defines a provider that can register plugins into a KernelBuilder.
/// </summary>
internal interface IAISKPluginProvider
{
    void Register(IKernelBuilder builder, IServiceProvider serviceProvider);
}

/// <summary>
/// A delegate-based implementation of <see cref="IAISKPluginProvider"/>.
/// </summary>
internal sealed class AISKDelegatePluginProvider(Action<IKernelBuilder, IServiceProvider> registrationAction) : IAISKPluginProvider
{
    private readonly Action<IKernelBuilder, IServiceProvider> _registrationAction = registrationAction;

    public void Register(IKernelBuilder builder, IServiceProvider serviceProvider)
    {
        _registrationAction(builder, serviceProvider);
    }
}
