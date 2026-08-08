using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Defines a provider that can register plugins into a KernelBuilder.
/// </summary>
internal interface IAISemanticKernelPluginProvider
{
    /// <summary>
    /// Registers plugins into the specified kernel builder.
    /// </summary>
    void Register(IKernelBuilder builder, IServiceProvider serviceProvider);
}
