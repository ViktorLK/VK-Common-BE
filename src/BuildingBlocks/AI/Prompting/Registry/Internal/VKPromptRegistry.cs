using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Prompting.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Registry.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPromptRegistry"/>.
/// It searches through all registered <see cref="IVKPromptProvider"/> instances in order.
/// </summary>
internal sealed class VKPromptRegistry : IVKPromptRegistry
{
    private readonly IEnumerable<IVKPromptProvider> _providers;
    private readonly IVKPromptTemplateEngine _templateEngine;
    private readonly ILogger<VKPromptRegistry> _logger;

    public VKPromptRegistry(
        IEnumerable<IVKPromptProvider> providers,
        IVKPromptTemplateEngine templateEngine,
        ILogger<VKPromptRegistry> logger)
    {
        _providers = VKGuard.NotNull(providers);
        _templateEngine = VKGuard.NotNull(templateEngine);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string>> RenderPromptAsync(
        string promptId,
        IDictionary<string, object?>? variables = null,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        var templateResult = await GetTemplateAsync(promptId, version, cancellationToken).ConfigureAwait(false);
        if (templateResult.IsFailure)
        {
            return VKResult.Failure<string>(templateResult.FirstError);
        }

        // Merge default variables with provided variables
        var template = templateResult.Value;
        var mergedVariables = new Dictionary<string, object?>(template.DefaultVariables);

        if (variables is not null)
        {
            foreach (var kvp in variables)
            {
                mergedVariables[kvp.Key] = kvp.Value;
            }
        }

        return await _templateEngine.RenderAsync(template.Text, mergedVariables, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<VKPromptTemplate>> GetTemplateAsync(
        string promptId,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(promptId);

        foreach (var provider in _providers)
        {
            var result = await provider.GetPromptAsync(promptId, version, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        PromptingLog.PromptNotFound(_logger, promptId, version ?? "latest");
        return VKResult.Failure<VKPromptTemplate>(VKError.NotFound("Prompt.NotFound", $"Prompt template '{promptId}' was not found."));
    }
}
