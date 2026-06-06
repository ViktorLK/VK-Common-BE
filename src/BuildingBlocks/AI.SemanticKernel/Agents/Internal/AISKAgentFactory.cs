using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

/// <summary>
/// Semantic Kernel implementation of <see cref="IVKAgentFactory"/>.
/// Follows AP.01 (sealed class, VKGuard).
/// </summary>
internal sealed class AISKAgentFactory : IVKAgentFactory
{
    private readonly IAISKKernelFactory _kernelFactory;
    private readonly VKAgentsOptions _options;
    private readonly VKAISKOptions _skOptions;

    public AISKAgentFactory(
        IAISKKernelFactory kernelFactory,
        IOptions<VKAgentsOptions> options,
        IOptions<VKAISKOptions> skOptions)
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

        return new AISKAgent(
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
        return new AISKAgentGroupRunner(kernel);
    }
}
