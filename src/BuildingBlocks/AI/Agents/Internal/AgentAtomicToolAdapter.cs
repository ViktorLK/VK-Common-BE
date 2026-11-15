using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Adapts an <see cref="IVKAgent"/> into an <see cref="IVKAtomicTool"/> so agents can be composed hierarchically as sub-agents.
/// </summary>
internal sealed class AgentAtomicToolAdapter : IVKAtomicTool
{
    private readonly IVKAgent _agent;
    private readonly VKAtomicToolManifest _manifest;

    public VKAtomicToolManifest Manifest => _manifest;

    public AgentAtomicToolAdapter(IVKAgent agent)
    {
        _agent = VKGuard.NotNull(agent);
        _manifest = new VKAtomicToolManifest
        {
            Metadata = new VKAtomicToolMetadata
            {
                Name = _agent.Name,
                Description = string.IsNullOrWhiteSpace(_agent.Description) ? $"Sub-agent '{_agent.Name}'" : _agent.Description
            },
            ParameterSchema = "{\"type\":\"object\",\"properties\":{\"input\":{\"type\":\"string\"}},\"required\":[\"input\"]}"
        };
    }

    public async Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        VKAtomicToolContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(arguments);

        var input = arguments.TryGetValue("input", out var val) ? val?.ToString() ?? string.Empty : string.Empty;
        var agentContext = new VKAgentExecutionContext
        {
            ToolContext = context ?? new VKAtomicToolContext(),
            TraceId = context?.CorrelationId
        };

        var result = await _agent.ExecuteAsync(input, agentContext, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return VKResult.Failure<VKAtomicToolResult>(result.FirstError);
        }

        return VKResult.Success(new VKAtomicToolResult { Content = result.Value ?? string.Empty });
    }
}
