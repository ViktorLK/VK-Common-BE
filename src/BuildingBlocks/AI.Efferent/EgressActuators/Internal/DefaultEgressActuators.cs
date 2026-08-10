using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressActuators.Internal;

internal sealed class DefaultEgressActuators : IVKEgressActuators
{
    private readonly VKEgressActuatorsOptions _options;
    private readonly ILogger<DefaultEgressActuators> _logger;

    public DefaultEgressActuators(
        IOptionsSnapshot<VKEgressActuatorsOptions> options,
        ILogger<DefaultEgressActuators> logger)
    {
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult<IReadOnlyList<VKToolResult>>> DispatchActionsAsync(IReadOnlyList<VKToolCall> toolCalls, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(toolCalls);
        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<VKToolResult>();

        foreach (var call in toolCalls)
        {
            _logger.LogInformation("Dispatching actuator execution for tool: {ToolName}, CallId: {CallId}", call.Name, call.Id);

            var result = new VKToolResult
            {
                CallId = call.Id,
                Name = call.Name,
                Content = "{\"status\": \"executed_by_efferent_actuator\"}",
                IsSuccess = true
            };
            results.Add(result);
        }

        IReadOnlyList<VKToolResult> readonlyResults = results;
        return Task.FromResult(VKResult.Success(readonlyResults));
    }
}
