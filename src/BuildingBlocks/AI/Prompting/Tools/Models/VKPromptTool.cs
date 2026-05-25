using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Tools;

/// <summary>
/// An atomic tool implementation that delegates execution to an inner prompt pipeline.
/// </summary>
public sealed class VKPromptTool : IVKAtomicTool
{
    private readonly VKPromptToolOptions _options;
    private readonly IVKPromptRegistry _promptRegistry;
    private readonly IVKChatEngine _chatEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKPromptTool"/> class.
    /// </summary>
    /// <param name="options">The tool options including the prompt ID and manifest.</param>
    /// <param name="promptRegistry">The prompt registry used to load and render the template.</param>
    /// <param name="chatEngine">The chat engine used to execute the rendered prompt.</param>
    public VKPromptTool(
        VKPromptToolOptions options,
        IVKPromptRegistry promptRegistry,
        IVKChatEngine chatEngine)
    {
        _options = VKGuard.NotNull(options);
        _promptRegistry = VKGuard.NotNull(promptRegistry);
        _chatEngine = VKGuard.NotNull(chatEngine);
    }

    /// <inheritdoc />
    public VKAtomicToolManifest Manifest => _options.Manifest;

    /// <inheritdoc />
    public async Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var (template, renderedPrompt) = await RenderPromptCoreAsync(arguments, cancellationToken).ConfigureAwait(false);
        if (renderedPrompt is null)
        {
            return VKResult.Failure<VKAtomicToolResult>(VKError.Failure("PromptTool.RenderFailed", "Failed to render the prompt template."));
        }

        // Use the Role defined in the PromptTemplate (typically System or User).
        // This is sent to the inner LLM that executes the Prompt Tool,
        // which is entirely separate from the outer LLM's Tool Role context.
        var message = new VKChatMessage
        {
            Role = template!.Role,
            Content = renderedPrompt
        };

        var messages = new[] { message };

        var responseResult = await _chatEngine.SendAsync(messages, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (responseResult.IsFailure)
        {
            return VKResult.Failure<VKAtomicToolResult>(responseResult.FirstError);
        }

        var toolResult = new VKAtomicToolResult
        {
            Content = responseResult.Value?.Message.Content ?? string.Empty
        };

        return VKResult.Success(toolResult);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<string>> ExecuteStreamingAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (template, renderedPrompt) = await RenderPromptCoreAsync(arguments, cancellationToken).ConfigureAwait(false);
        if (renderedPrompt is null)
        {
            yield return VKResult.Failure<string>(VKError.Failure("PromptTool.RenderFailed", "Failed to render the prompt template."));
            yield break;
        }

        var message = new VKChatMessage
        {
            Role = template!.Role,
            Content = renderedPrompt
        };

        var messages = new[] { message };

        await foreach (var chunkResult in _chatEngine.SendStreamingAsync(messages, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (chunkResult.IsFailure)
            {
                yield return VKResult.Failure<string>(chunkResult.FirstError);
                yield break;
            }

            yield return VKResult.Success(chunkResult.Value?.Delta ?? string.Empty);
        }
    }

    private async Task<(VKPromptTemplate? Template, string? RenderedText)> RenderPromptCoreAsync(
        IDictionary<string, object> arguments,
        CancellationToken cancellationToken)
    {
        var templateResult = await _promptRegistry.GetTemplateAsync(_options.PromptId, _options.PromptVersion, cancellationToken).ConfigureAwait(false);
        if (templateResult.IsFailure)
        {
            return (null, null);
        }

        var renderArguments = new Dictionary<string, object?>();
        foreach (var kvp in arguments)
        {
            renderArguments[kvp.Key] = kvp.Value;
        }

        var renderedResult = await _promptRegistry.RenderPromptAsync(_options.PromptId, renderArguments, _options.PromptVersion, cancellationToken).ConfigureAwait(false);
        if (renderedResult.IsFailure)
        {
            return (null, null);
        }

        return (templateResult.Value, renderedResult.Value);
    }
}
