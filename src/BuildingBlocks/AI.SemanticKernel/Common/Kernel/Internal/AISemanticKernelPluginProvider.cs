using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// A delegate-based implementation of <see cref="IAISemanticKernelPluginProvider"/>.
/// </summary>
internal sealed class AISemanticKernelDelegatePluginProvider(Action<IKernelBuilder, IServiceProvider> registrationAction) : IAISemanticKernelPluginProvider
{
    private readonly Action<IKernelBuilder, IServiceProvider> _registrationAction = registrationAction;

    public void Register(IKernelBuilder builder, IServiceProvider serviceProvider)
    {
        _registrationAction(builder, serviceProvider);
    }
}
