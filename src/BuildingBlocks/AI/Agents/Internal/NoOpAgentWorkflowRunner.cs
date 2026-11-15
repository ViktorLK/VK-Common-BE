using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// No-op fallback implementation of <see cref="IVKAgentWorkflowRunner"/> when no agent workflow engine is registered.
/// </summary>
internal sealed class NoOpAgentWorkflowRunner : IVKAgentWorkflowRunner
{
    public static readonly NoOpAgentWorkflowRunner Instance = new();

    private NoOpAgentWorkflowRunner() { }

    /// <inheritdoc />
    public Task<VKResult<VKAgentWorkflowResult>> ExecuteWorkflowAsync(
        VKAgentWorkflowDefinition definition,
        string input,
        string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKAgentWorkflowResult>(VKCoreErrors.Infrastructure.ProviderNotConfigured)); // [CS.01]
    }

    /// <inheritdoc />
    public Task<VKResult<VKAgentWorkflowResult>> ResumeWorkflowAsync(
        VKAgentWorkflowDefinition definition,
        string sessionId,
        string? checkpointId = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKAgentWorkflowResult>(VKCoreErrors.Infrastructure.ProviderNotConfigured)); // [CS.01]
    }
}
