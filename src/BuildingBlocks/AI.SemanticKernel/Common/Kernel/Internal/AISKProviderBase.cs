using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Base class for Semantic Kernel based providers.
/// Following conversation eee87fe8-44ba-4fa7-b176-cbc8cef7a6c9: removed VK prefix.
/// </summary>
public abstract class AISKProviderBase
{
    protected Microsoft.SemanticKernel.Kernel Kernel { get; }
    protected string ModelName { get; }

    protected AISKProviderBase(Microsoft.SemanticKernel.Kernel kernel, string modelName)
    {
        Kernel = VKGuard.NotNull(kernel);
        ModelName = VKGuard.NotNull(modelName);
    }
}
