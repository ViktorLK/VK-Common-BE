using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Defines a provider that can register plugins into a KernelBuilder.
/// </summary>
public interface IVKAISKPluginProvider
{
    /// <summary>
    /// Registers plugins into the specified kernel builder.
    /// </summary>
    void Register(IKernelBuilder builder, IServiceProvider serviceProvider);
}
