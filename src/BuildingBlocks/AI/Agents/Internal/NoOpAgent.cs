using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKAgent"/> for graceful disablement when no agent provider is registered.
/// </summary>
internal sealed class NoOpAgent : IVKAgent
{
    public static readonly NoOpAgent Instance = new();

    private NoOpAgent() { }

    /// <inheritdoc />
    public string Name => "NoOpAgent";

    /// <inheritdoc />
    public string Description => string.Empty;

    /// <inheritdoc />
    public string Instructions => string.Empty;

    /// <inheritdoc />
    public IReadOnlyList<IVKAtomicTool> Tools => [];

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Metadata => FrozenDictionary<string, object>.Empty;

    /// <inheritdoc />
    public Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<string>(VKCoreErrors.Infrastructure.ProviderNotConfigured)); // [CS.01]
    }
}
