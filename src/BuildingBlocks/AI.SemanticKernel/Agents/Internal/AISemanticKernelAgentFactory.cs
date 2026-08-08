using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

/// <summary>
/// Semantic Kernel implementation of <see cref="IVKAgentFactory"/>.
/// Follows AP.01 (sealed class, VKGuard).
/// </summary>
internal sealed class AISemanticKernelAgentFactory : IVKAgentFactory
{
    private readonly IAISemanticKernelKernelFactory _kernelFactory;
    private readonly VKAgentsOptions _options;
    private readonly VKAISemanticKernelOptions _skOptions;

    public AISemanticKernelAgentFactory(
        IAISemanticKernelKernelFactory kernelFactory,
        IOptions<VKAgentsOptions> options,
        IOptions<VKAISemanticKernelOptions> skOptions)
    {
        _kernelFactory = VKGuard.NotNull(kernelFactory);
        _options = VKGuard.NotNull(options?.Value);
        _skOptions = VKGuard.NotNull(skOptions?.Value);
    }

    /// <inheritdoc />
    public IVKAgent CreateAgent(
        string name,
        string description,
        string instructions = "",
        IEnumerable<IVKAtomicTool>? tools = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name); // [AP.01]
        VKGuard.NotNullOrWhiteSpace(description); // [AP.01]

        var kernel = _kernelFactory.CreateKernel();
        var modelName = _skOptions.DeploymentName ?? "Unknown";

        return new AISemanticKernelAgent(
            kernel,
            modelName,
            name,
            description,
            instructions,
            _options,
            tools,
            metadata
        );
    }

    /// <inheritdoc />
    public IVKAgentGroup CreateAgentGroup()
    {
        var kernel = _kernelFactory.CreateKernel();
        return new AISemanticKernelAgentGroupRunner(kernel);
    }
}
