using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKAgentGroup"/> for graceful disablement when no agent provider is registered.
/// </summary>
internal sealed class NoOpAgentGroup : IVKAgentGroup
{
    public static readonly NoOpAgentGroup Instance = new();

    private NoOpAgentGroup() { }

    /// <inheritdoc />
    public Task<VKResult<VKAgentGroupResult>> ExecuteAsync(
        string input,
        IReadOnlyList<IVKAgent> agents,
        VKAgentGroupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKAgentGroupResult>(VKCoreErrors.Infrastructure.ProviderNotConfigured)); // [CS.01]
    }
}
